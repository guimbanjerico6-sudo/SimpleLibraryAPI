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
            var book = _bookService.GetByTitle(title);
            if (book == null) return NotFound($"Book '{title}' not found.");
            return Ok(book);
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
                CurrentBorrowerIds = new List<int>()
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

        [HttpPut("borrow/{title}")]
        public IActionResult Borrow(string title, [FromQuery] string userName, [FromQuery] string cardNum)
        {
            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(cardNum))
                return BadRequest("User name and Card Number are required.");

            try
            {
                _bookService.BorrowBook(title, userName, cardNum);

                return Ok(new
                {
                    Message = $"Book borrowed successfully by {userName}!",
                    Details = _bookService.GetByTitle(title)
                });
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found")) return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Return/{title}")]
        public IActionResult Return(string title, [FromQuery] int userId)
        {
            try
            {
                _bookService.ReturnBook(title, userId);

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