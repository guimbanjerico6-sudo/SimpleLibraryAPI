using Microsoft.AspNetCore.Http.HttpResults;
using SimpleLibraryAPI.Models;

namespace SimpleLibraryAPI.Services
{
    public class BookService
    {
        private static List<Book> _books = new List<Book>();
        private static List<ActivityLog> _history = new List<ActivityLog>();
        private static List<User> _users = new List<User>(); // For future user management

        //get all books
        public List<Book> GetAllBooks() => _books;
        //short cut for return _books;
        public List<User> GetAllUsers() => _users;


        // get book by title
        public Book GetByTitle(string title) => _books.FirstOrDefault(b => b.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase));

        public List<Book> GetByAuthor(string author)
        {
            // .Where() finds EVERY match, not just the first one
            return _books.Where(b => b.Author.Equals(author, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        // add new book
        public string AddBook(Book newBook)
        {
            if (_books.Any(b => b.BookTitle.Equals(newBook.BookTitle, StringComparison.OrdinalIgnoreCase)))
                return "Duplicate";

            if (newBook.Stock <= 0) return "NegativeStock";

            newBook.MaxStock = newBook.Stock;
            _books.Add(newBook);

            // Logic: Log the addition
            LogActivity("Add", newBook.BookTitle, $"Initial inventory of {newBook.Stock} copies added.");
            return "Success";
        }

        // update stock
        public Book? AdminExpansion(string title, int amountToAdd)
        {
            var book = GetByTitle(title);
            if (book == null) return null;

            book.Stock += amountToAdd;
            book.MaxStock += amountToAdd;

            // Logic: Log the expansion
            LogActivity("Admin Expansion", title, $"Inventory expanded by {amountToAdd} copies.");
            return book;
        }

        // delete book
        public bool DeleteBook(string title)
        {
            var book = GetByTitle(title);
            if (book == null) return false;

            _books.Remove(book);
            return true;
        }


        public string BorrowBook(string title, string userName, string cardNum)
        {
            var book = GetByTitle(title);
            if (book == null) return "NotFound";
            if (book.Stock <= 0) return "OutOfStock";

            // Identify or Onboard by unique Card Number
            var user = GetOrOnboardUser(userName, cardNum);

            book.Stock--;
            book.CurrentBorrowerIds.Add(user.UserId);

            // Using your LogActivity helper for cleaner code
            LogActivity("Borrow", title, $"Borrowed by {user.FullName} (ID: {user.UserId})", user.UserId);

            return "Success";
        }


        public string ReturnBook(string title, int userId)
        {
            var book = GetByTitle(title);
            if (book == null) return "NotFound";

            if (!book.CurrentBorrowerIds.Contains(userId))
            {
                return "NotTheBorrower";
            }

            // Identify the user for the history details
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            string userName = user?.FullName ?? "Unknown User";

            book.Stock++;
            book.CurrentBorrowerIds.Remove(userId);

            // Logic: Log the return with the user's name
            LogActivity("Return", title, $"Returned by {userName} (ID: {userId})", userId);
            return "Success";
        }



        private void LogActivity(string action, string title, string details, int? userId = null)
        {
            _history.Add(new ActivityLog
            {
                Action = action,
                BookTitle = title,
                Timestamp = DateTime.Now,
                BorrowerId = userId ?? 0, // Use the ID if provided, otherwise 0
                Details = details
            });
        }
        public List<ActivityLog> GetHistory() => _history;
        private User GetOrOnboardUser(string userName, string cardNum)
        {
            // Check by Card Number instead of Name, because Card Numbers are UNIQUE
            var user = _users.FirstOrDefault(u => u.LibraryCard == cardNum);

            if (user == null)
            {
                user = new User
                {
                    UserId = _users.Count + 1,
                    FullName = userName,
                    LibraryCard = cardNum
                };
                _users.Add(user);
            }
            return user;
        }

    }

}