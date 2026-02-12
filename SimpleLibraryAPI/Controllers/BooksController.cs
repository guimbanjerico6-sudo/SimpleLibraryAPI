using Microsoft.AspNetCore.Mvc;
using SimpleLibraryAPI.Models;
using SimpleLibraryAPI.Services;

namespace SimpleLibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;

        public BooksController()
        {
            _bookService = new BookService();
        }

        [HttpGet]
        public List<Book> Get() => _bookService.GetAllBooks();

        [HttpGet("{title}")]
        public Book Get(string title) => _bookService.GetByTitle(title);

        [HttpPost]
        public string Post(Book newBook)
        {
            var result = _bookService.AddBook(newBook);

            return result switch
            {
                "Duplicate" => $"Sorry, '{newBook.BookTitle}' is already in the list.",
                "NegativeStock" => "Stock cannot be negative.",
                _ => $"Success! '{newBook.BookTitle}' added."
            };
        }

        [HttpPut("{title}")]
        public string Put(string title, int newStock)
        {
            var success = _bookService.UpdateStock(title, newStock);
            return success ? "Stock updated!" : $"'{title}' not found.";
        }

        [HttpDelete("{title}/{passcode}")]
        public string Delete(string title, string passcode)
        {
            if (passcode != "admin123") return "Unauthorized!";

            var success = _bookService.DeleteBook(title);
            return success ? "Book removed." : "Book not found.";
        }
    }
}