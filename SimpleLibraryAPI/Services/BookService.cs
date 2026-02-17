using System.Text.Json;
using System.IO;
using SimpleLibraryAPI.Models;

namespace SimpleLibraryAPI.Services
{
    public class BookService
    {
        // In-memory data storage (replaced with file-based persistence)
        private List<Book> _books = new List<Book>();
        private List<ActivityLog> _history = new List<ActivityLog>();
        private List<User> _users = new List<User>();

        // File paths for data persistence
        private readonly string _booksFile = "books.json";
        private readonly string _usersFile = "users.json";
        private readonly string _historyFile = "history.json";

        public BookService()
        {
            _books = LoadDataFromFile<Book>(_booksFile);
            _users = LoadDataFromFile<User>(_usersFile);
            _history = LoadDataFromFile<ActivityLog>(_historyFile);
        }

        // --- PERSISTENCE HELPERS ---
        private void SaveAll()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_booksFile, JsonSerializer.Serialize(_books, options));
            File.WriteAllText(_usersFile, JsonSerializer.Serialize(_users, options));
            File.WriteAllText(_historyFile, JsonSerializer.Serialize(_history, options));
        }

        private List<T> LoadDataFromFile<T>(string fileName)
        {
            if (!File.Exists(fileName)) return new List<T>();
            try
            {
                string json = File.ReadAllText(fileName);
                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            catch { return new List<T>(); }
        }

        // --- CORE METHODS ---
        public List<Book> GetAllBooks() => _books;
        public List<User> GetAllUsers() => _users;
        public List<ActivityLog> GetHistory() => _history;

        // Get book by title
        public Book GetByTitle(string title) =>
            _books.FirstOrDefault(b => b.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase));

        public List<Book> GetByAuthor(string author) =>
            _books.Where(b => b.Author.Equals(author, StringComparison.OrdinalIgnoreCase)).ToList();

        public string AddBook(Book newBook)
        {
            if (_books.Any(b => b.BookTitle.Equals(newBook.BookTitle, StringComparison.OrdinalIgnoreCase)))
                return "Duplicate";

            // If MaxStock isn't provided in JSON, default it to current Stock
            if (newBook.MaxStock == 0) newBook.MaxStock = newBook.Stock;

            _books.Add(newBook);
            LogActivity("Add", newBook.BookTitle, "New book added to library.");
            SaveAll();
            return "Success";
        }

        public Book? AdminExpansion(string title, int amountToAdd)
        {
            var book = GetByTitle(title);
            if (book == null) return null;

            book.Stock += amountToAdd;
            book.MaxStock += amountToAdd;

            LogActivity("Admin Expansion", title, $"Inventory expanded by {amountToAdd} copies.");
            SaveAll();
            return book;
        }

        public bool DeleteBook(string title)
        {
            var book = GetByTitle(title);
            if (book == null) return false;

            _books.Remove(book);
            LogActivity("Delete", title, "Book removed from inventory.");
            SaveAll();
            return true;
        }

        public string BorrowBook(string title, string userName, string cardNum)
        {
            var book = GetByTitle(title);
            if (book == null) return "NotFound";
            if (book.Stock <= 0) return "OutOfStock";

            var user = GetOrOnboardUser(userName, cardNum);

            book.Stock--;
            book.CurrentBorrowerIds.Add(user.UserId);

            LogActivity("Borrow", title, $"Borrowed by {user.FullName}", user.UserId);
            SaveAll();
            return "Success";
        }

        public string ReturnBook(string title, int userId)
        {
            var book = GetByTitle(title);
            if (book == null) return "NotFound";
            if (!book.CurrentBorrowerIds.Contains(userId)) return "NotTheBorrower";

            book.Stock++;
            book.CurrentBorrowerIds.Remove(userId);

            LogActivity("Return", title, $"Returned by User ID: {userId}", userId);
            SaveAll();
            return "Success";
        }

        private User GetOrOnboardUser(string userName, string cardNum)
        {
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

        private void LogActivity(string action, string title, string details, int? userId = null)
        {
            _history.Add(new ActivityLog
            {
                Action = action,
                BookTitle = title,
                Timestamp = DateTime.Now,
                BorrowerId = userId ?? 0,
                Details = details
            });
        }
        public string AddUser(string name)
        {
            string newCardNum;

            if (!_users.Any())
            {
                newCardNum = "LIB-0001";
            }
            else
            {
                int currentMax = _users.Max(u => int.Parse(u.LibraryCard.Substring(4)));

                // 4. Add 1 and format it back to "LIB-0006"
                newCardNum = $"LIB-{(currentMax + 1).ToString("D4")}";
            }

            // 2. GENERATE ID (Sequential) - This stays the same
            int newId = _users.Any() ? _users.Max(u => u.UserId) + 1 : 1;

            // 3. CREATE AND SAVE
            var newUser = new User
            {
                UserId = newId,
                FullName = name,
                LibraryCard = newCardNum
            };

            _users.Add(newUser);
            SaveAll(); // Auto-save to users.json

            return newCardNum;
        }

    }
}