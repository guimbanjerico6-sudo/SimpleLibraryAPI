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

        
        public BooksController(BookService bookService)
        {
            _bookService = bookService;

        }

        /// <summary>
        /// View all Book information
        /// </summary>
        [HttpGet]
        public List<Book> Get() => _bookService.GetAllBooks();

        ///<summary>
        /// Find User
        /// <summary>
        [HttpGet("users")]
        public List<User> GetUsers() => _bookService.GetAllUsers();


        /// <summary>
        /// Find Book With Title 
        /// </summary>
        [HttpGet("{title}")]
        public IActionResult Get(string title)
        {
            var book = _bookService.GetByTitle(title);

            if (book == null)
            {
                return NotFound($"Book with Title {title} was not found.");
            }
            return Ok(book);
        }

        /// <summary>
        /// Find a specific book by its author
        /// </summary>
        [HttpGet("author")]
        public List<Book> GetByAuthor([FromQuery] string name)
        {
            // We just return the list directly. 
            // If it's empty, Swagger will show [] and a 200 OK status.
            return _bookService.GetByAuthor(name);
        }

        /// <summary>
        /// Add a new book to the library
        /// </summary>
        [HttpPost]
        public IActionResult Post(Book newBook)
        {
            var success = _bookService.AddBook(newBook);

            return success switch
            {
                "Duplicate" => Conflict("A book with the same title already exists."),
                "NegativeStock" => BadRequest("Stock cannot be negative."),
                _ => CreatedAtAction(nameof(Get), new { title = newBook.BookTitle }, newBook)
            };
        }

        /// <summary>
        /// Increase inventory capacity (Admin Only)
        /// </summary>
        [HttpPut("admin/inventory/{title}")]
        public IActionResult AdminUpdate(
        string title,
        [FromQuery] int amountToAdd,
        [FromHeader(Name = "X-Admin-Password")] string password) // Secret Header
        {
            // The Guard
            if (password != "Admin123")
            {
                return Unauthorized("Access Denied: Invalid Admin Password.");
            }

            // The rest is Scenario 2 logic (Stock + MaxStock)
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

        /// <summary>
        /// Delete Book
        /// </summary>
        [HttpDelete("{title}")]
        public IActionResult Delete(string title)
        {
            var success = _bookService.DeleteBook(title);
            if (!success)
            {
                return NotFound($"Book with Title {title} was not found.");
            }
            return Ok("Book deleted successfully.");

        }

        /// <summary>
        /// Borrow Book (Strict: Must provide both User Name and Card Number)
        /// </summary>
        [HttpPut("borrow/{title}")]
        public IActionResult Borrow(string title, [FromQuery] string userName, [FromQuery] string cardNum)
        {
            // Fix: Check both name and card number strings
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(cardNum))
                return BadRequest("User name and Card Number are required to borrow a book.");

            // Pass the cardNum to the service
            var result = _bookService.BorrowBook(title, userName, cardNum);

            return result switch
            {
                "NotFound" => NotFound($"Book '{title}' not found."),
                "OutOfStock" => BadRequest("This book is currently out of stock."),
                "Success" => Ok(new
                {
                    Message = $"Book borrowed successfully by {userName} (Card: {cardNum})!",
                    Details = _bookService.GetByTitle(title)
                }),
                _ => StatusCode(500, "Unexpected error.")
            };
        }

        /// <summary>
        /// Return Book (Strict: Must provide the specific User ID)
        /// </summary>
        [HttpPut("Return/{title}")]
        public IActionResult Return(string title, [FromQuery] int userId)
        {
            // Now correctly passing the userId to your Strict service logic
            var result = _bookService.ReturnBook(title, userId);

            return result switch
            {
                "NotFound" => NotFound($"Book with Title '{title}' was not found."),
                "NotTheBorrower" => BadRequest("Security Alert: This User ID is not recorded as a borrower of this book."),
                "Success" => Ok(new
                {
                    Message = "Book returned successfully!",
                    Details = _bookService.GetByTitle(title)
                }),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }
        /// <summary>
        /// View the full history of library activities
        /// </summary>
        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            return Ok(_bookService.GetHistory());
        }    
    }
}