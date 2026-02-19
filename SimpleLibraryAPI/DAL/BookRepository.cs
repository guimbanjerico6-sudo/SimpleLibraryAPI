using SimpleLibraryAPI.Models;
using System.Text.Json;

namespace SimpleLibraryAPI.DAL
{
    public class BookRepository
    {
        private List<Book> _books = new List<Book>();
        private List<User> _users = new List<User>();
        private List<ActivityLog> _history = new List<ActivityLog>();

        private readonly string _booksFile = "books.json";
        private readonly string _usersFile = "users.json";
        private readonly string _historyFile = "history.json";

        public BookRepository()
        {
            _books = LoadDataFromFile<Book>(_booksFile);
            _users = LoadDataFromFile<User>(_usersFile);
            _history = LoadDataFromFile<ActivityLog>(_historyFile);
        }

        // --- CORE STORAGE (The "Warehouse") ---
        public void SaveChanges()
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

        // --- ACCESSORS ---
        public List<Book> GetAllBooks() => _books;
        public List<User> GetAllUsers() => _users;
        public List<ActivityLog> GetHistory() => _history;

        public Book? GetBookByTitle(string title) =>
            _books.FirstOrDefault(b => b.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase));

        public User? GetUserByCard(string cardNum) =>
            _users.FirstOrDefault(u => u.LibraryCard == cardNum);

        // --- MODIFIERS ---
        public void AddBook(Book book)
        {
            _books.Add(book);
            SaveChanges();
        }

        public void RemoveBook(Book book)
        {
            _books.Remove(book);
            SaveChanges();
        }

        public void AddUser(User user)
        {
            _users.Add(user);
            SaveChanges();
        }

        public void AddLog(ActivityLog log)
        {
            _history.Add(log);
            SaveChanges();
        }
    }
}