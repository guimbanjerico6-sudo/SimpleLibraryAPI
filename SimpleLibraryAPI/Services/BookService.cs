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
        public Book? GetByTitle(string title) => _repository.GetBookByTitle(title);

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

        public void BorrowBook(string title, string userName, string cardNum)
        {
            var book = _repository.GetBookByTitle(title);

            if (book == null) throw new ArgumentException($"Book '{title}' not found.");

            if (book.Stock <= 0)
            {
                throw new ArgumentException("This book is currently out of stock.");
            }

            var user = GetOrOnboardUser(userName, cardNum);

            book.Stock--;
            book.CurrentBorrowerIds.Add(user.UserId);

            _repository.SaveChanges(); 
            LogActivity("Borrow", title, $"Borrowed by {user.FullName}", user.UserId);
        }

        public void ReturnBook(string title, int userId)
        {
            var book = _repository.GetBookByTitle(title);
            if (book == null) throw new ArgumentException($"Book '{title}' not found.");

            if (!book.CurrentBorrowerIds.Contains(userId))
            {
                throw new ArgumentException("This user does not have this book.");
            }

            book.Stock++;
            book.CurrentBorrowerIds.Remove(userId);

            _repository.SaveChanges();
            LogActivity("Return", title, $"Returned by User ID: {userId}", userId);
        }

        // --- HELPER LOGIC ---

        private User GetOrOnboardUser(string userName, string cardNum)
        {
            var user = _repository.GetUserByCard(cardNum);
            if (user == null)
            {
                var allUsers = _repository.GetAllUsers();
                user = new User
                {
                    UserId = allUsers.Count + 1,
                    FullName = userName,
                    LibraryCard = cardNum
                };
                _repository.AddUser(user);
            }
            return user;
        }

        public string AddUser(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("User name cannot be empty.");
            }

            var users = _repository.GetAllUsers();
            string newCardNum;

            if (!users.Any())
            {
                newCardNum = "LIB-0001";
            }
            else
            {
                int currentMax = users.Max(u => int.Parse(u.LibraryCard.Substring(4)));
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

        private void LogActivity(string action, string title, string details, int? userId = null)
        {
            var log = new ActivityLog
            {
                Action = action,
                BookTitle = title,
                Timestamp = DateTime.Now,
                BorrowerId = userId ?? 0,
                Details = details
            };
            _repository.AddLog(log);
        }
    }
}