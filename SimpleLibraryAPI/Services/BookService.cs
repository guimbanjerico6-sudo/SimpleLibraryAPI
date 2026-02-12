using SimpleLibraryAPI.Models;

namespace SimpleLibraryAPI.Services
{
    public class BookService
    {
        private static List<Book> _books = new List<Book>();
        
        public List<Book> GetAllBooks() => _books;
        //short cut for return _books;

        public Book GetByTitle(string title)
        {
            return _books.FirstOrDefault(b => b.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        public string AddBook(Book newBook)
        {
            if (_books.Any(b => b.BookTitle.Equals(newBook.BookTitle, StringComparison.OrdinalIgnoreCase)))
                return "Duplicate";

            if (newBook.Stock < 0)
                return "NegativeStock";

            _books.Add(newBook);
            return "Success";
        }

        public bool UpdateStock(string title, int newStock)
        {
            var book = GetByTitle(title);
            if (book == null) return false;
            if (newStock < 0) return false;

            book.Stock = newStock;
            return true;
        }

        public bool DeleteBook(string title)
        {
            var book = GetByTitle(title);
            if (book == null) return false;

            _books.Remove(book);
            return true;
        }
    }
}