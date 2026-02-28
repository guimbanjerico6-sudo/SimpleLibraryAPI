using SimpleLibraryAPI.Models;
using SimpleLibraryAPI.DAL;

namespace SimpleLibraryAPI.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;

        public BookService(IBookRepository repository)
        {
            _repository = repository;
        }

        // --- READ OPERATIONS ---
        public List<Book> GetBooks() => _repository.GetAllBooks();
        public List<User> GetAllUsers() => _repository.GetAllUsers();
        public List<ActivityLog> GetHistory() => _repository.GetHistory();
        public Book? GetByTitle(string title)
        {
            var book = _repository.GetBookByTitle(title);
            
            if(book == null)
            {
                throw new ArgumentException($"Book '{title}' not found.");
            }
            return book;
        }

        public List<Book> GetByAuthor(string author) =>
            _repository.GetAllBooks()
                .Where(b => b.Author.Equals(author, StringComparison.OrdinalIgnoreCase))
                .ToList();

        // --- WRITE OPERATIONS (The Logic) ---

        public void AddBook(Book newBook)
        {
            if (newBook.Stock < 0)
            {
                throw new ArgumentException("Stock cannot be negative.");
            }

            var existing = _repository.GetBookByTitle(newBook.BookTitle);
            if (existing != null)
            {
                throw new ArgumentException("Book title already exists.");
            }
            _repository.AddBook(newBook);
            LogActivity("Add", newBook.BookTitle, "New book added to library.");
        }

        public Book? AdminExpansion(string title, int amountToAdd)
        {
            if (amountToAdd <= 0)
            {
                throw new ArgumentException("Amount to add must be greater than zero.");
            }

            var book = _repository.GetBookByTitle(title);
            if (book == null) return null;

            // 3. UPDATE & SAVE
            book.Stock += amountToAdd;
            book.MaxStock += amountToAdd;

            _repository.SaveChanges(); // Persist changes

            LogActivity("Admin Expansion", title, $"Inventory expanded by {amountToAdd} copies.");
            return book;
        }

        public bool DeleteBook(string title)
        {
            var book = _repository.GetBookByTitle(title);
            if (book == null) return false;

            _repository.RemoveBook(book);
            LogActivity("Delete", title, "Book removed from inventory.");
            return true;
        }

        public void BorrowBook(string libraryCard, string title)
        {
            if (string.IsNullOrWhiteSpace(libraryCard))
            {
                throw new ArgumentException("Library card cannot be empty.");
            }

            User borrower = _repository.GetUserByLibraryCard(libraryCard);


            if (borrower == null)
            {
                throw new ArgumentException($"No user found with Library Card: {libraryCard}");
            }

            var allLogs = _repository.GetHistory();

            var lastBookAction = allLogs
                .Where(log => log.BorrowerLibCard == libraryCard &&
                log.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(log => log.Timestamp) 
                .FirstOrDefault(); 

            if (lastBookAction != null && lastBookAction.Action == "Borrow")
            {
                throw new Exception($"Access Denied: You are already borrowing '{title}'. Please return it first.");
            }


            var book = _repository.GetBookByTitle(title);
            if (book == null)
            {
                throw new ArgumentException($"Book '{title}' not found.");
            }

            book.Stock--;
            book.CurrentBorrowerLibraryCard.Add(borrower.LibraryCard);

            _repository.SaveChanges();
            LogActivity("Borrow", title, $"Borrowed by {borrower.FullName}", borrower.LibraryCard);

        }

        public void ReturnBook(string libraryCard, string title)
        {
            if (string.IsNullOrWhiteSpace(libraryCard))
            {
                throw new ArgumentException("Library card cannot be empty.");
            }
            User CurrentBorrower = _repository.GetUserByLibraryCard(libraryCard);

            if (CurrentBorrower == null)
            {
                throw new ArgumentException($"No user found with Library Card: {libraryCard}");
            }
            var book = _repository.GetBookByTitle(title);
            if (book == null)
            {
                throw new ArgumentException($"Book '{title}' not found.");
            }
            if (!book.CurrentBorrowerLibraryCard.Contains(libraryCard))
            {
                throw new ArgumentException("This user does not have this book.");
            }

            book.Stock++;
            book.CurrentBorrowerLibraryCard.Remove(libraryCard);
            _repository.SaveChanges();
            LogActivity("Return", title, $"Returned Libraty card ID: {libraryCard}", libraryCard);
        }


        public string AddUser(string name)
        {
            // 1. FRONT DOOR BOUNCER (Restored from your original code)
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("User name cannot be empty.");
            }

            var users = _repository.GetAllUsers();
            string newCardNum;

            var validUsers = users.Where(u =>
                !string.IsNullOrWhiteSpace(u.LibraryCard) &&
                u.LibraryCard.Length > 4 &&
                u.LibraryCard.StartsWith("LIB-")
            ).ToList();

            if (!validUsers.Any())
            {
                newCardNum = "LIB-0001";
            }
            else
            {
                int currentMax = validUsers.Max(u => int.Parse(u.LibraryCard.Substring(4)));
                newCardNum = $"LIB-{(currentMax + 1).ToString("D4")}";
            }

            int newId = users.Any() ? users.Max(u => u.UserId) + 1 : 1;

            var newUser = new User
            {
                UserId = newId,
                FullName = name,
                LibraryCard = newCardNum
            };

            _repository.AddUser(newUser);

            return newCardNum;
        }

        private void LogActivity(string action, string title, string details, string libCard = null)
        {
            var log = new ActivityLog
            {
                Action = action,
                BookTitle = title,
                Timestamp = DateTime.Now,

                // This works perfectly now! Both sides are strings.
                BorrowerLibCard = libCard ?? "None",

                Details = details
            };
            _repository.AddLog(log);
        }
    }
}