using Microsoft.AspNetCore.Mvc;
using SimpleLibraryAPI.Models;
using SimpleLibraryAPI.Services;

namespace SimpleLibraryAPI.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public List<Book> Get() => _bookService.GetBooks();

        [HttpGet("users")]
        public List<User> GetUsers() => _bookService.GetAllUsers();

        [HttpGet("{title}")]
        public IActionResult Get(string title)
        {
            try
            {
                var book = _bookService.GetByTitle(title);
                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("author")]
        public List<Book> GetByAuthor([FromQuery] string name)
        {
            return _bookService.GetByAuthor(name);
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            return Ok(_bookService.GetHistory());
        }

        [HttpPost]
        public IActionResult AddBook([FromBody] AddBookRequest request)
        {

            var newBook = new Book
            {
                BookTitle = request.BookTitle,
                Author = request.Author,
                Stock = request.Stock,
                MaxStock = request.Stock, // Default logic
                CurrentBorrowerLibraryCard = new List<string>()
            };

            try
            {
                
                _bookService.AddBook(newBook);

                
                return CreatedAtAction(nameof(Get), new { title = newBook.BookTitle }, newBook);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }   

        [HttpPost("user")]
        public IActionResult AddUser([FromBody] AddUserRequest request)
        {
            try
            {
                string newCard = _bookService.AddUser(request.FullName);
                return Ok(new
                {
                    Message = "Welcome to the library!",
                    CardNumber = newCard,
                    Owner = request.FullName
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("admin/inventory/{title}")]
        public IActionResult AdminUpdate(string title, [FromQuery] int amountToAdd, [FromHeader(Name = "X-Admin-Password")] string password)
        {
            try
            {
                if (password != "Admin123") return Unauthorized("Access Denied.");

                var success = _bookService.AdminExpansion(title, amountToAdd);

                if (success == null) return NotFound("Book not found.");

                return Ok(new
                {
                    Message = "Inventory expanded successfully!",
                    NewStock = success.Stock,
                    NewMaxCapacity = success.MaxStock,
                    Book = success.BookTitle
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{title}")]
        public IActionResult Delete(string title)
        {
            var success = _bookService.DeleteBook(title);
            if (!success) return NotFound($"Book '{title}' not found.");
            return Ok("Book deleted successfully.");
        }

        [HttpPut("{libraryCard}/borrow")]
        public IActionResult BorrowBook(string libraryCard, string title)
        {
            try
            {
                // We pass the string directly to the Service
                _bookService.BorrowBook(libraryCard, title);
                return Ok($"Successfully borrowed book for card {libraryCard}.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Return/{librarycard}")]
        public IActionResult Return(string librarycard, string title)
        {
            try
            {
                _bookService.ReturnBook(librarycard, title);

                return Ok(new
                {
                    Message = "Book returned successfully!",
                    Details = _bookService.GetByTitle(title)
                });
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found")) return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}