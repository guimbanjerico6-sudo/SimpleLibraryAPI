using Microsoft.AspNetCore.Mvc;
using SimpleLibraryAPI.Models;

namespace SimpleLibraryAPI.Controllers
{
    [Route("api/[controller]")] // This makes the URL: api/books
    [ApiController]
    public class BooksController : ControllerBase
    {
        // Our "Fake Database"
        private static List<Book> books = new List<Book> { };


        // This is a "GET" request - it just reads the data
        [HttpGet]
        public List<Book> Get()
        {
            return books;
        }
        [HttpPost]
        public string Post(Book newBook)
        {
            if (books.Any(item => item.BookTitle == newBook.BookTitle))
            {
                return $"Sorry, '{newBook.BookTitle}' is already in the API list, so it cannot be added again.";
            }
            if (newBook.Stock < 0)
            {
                return $"Sorry, '{newBook.BookTitle}' cannot be added to the API list because stock cannot be negative.";
            }
            books.Add(newBook);

            // If 'BookTitle' is red, ensure it is spelled exactly 
            // the same way in your Book.cs file!
            return $"Success! '{newBook.BookTitle}' has been added to the API list.";
        }

        [HttpGet("{title}")]
        public Book Get(string title) // Changed return type from string to Book
        {
            var isBookExist = books.FirstOrDefault(b => b.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase));

            // If found, return the whole object. If not, return null.
            return isBookExist;
        }

        [HttpDelete("{title}")]
        public string Delete(string title)
        {
            // FIX: Again, search the 'BookTitle' property specifically
            var bookToDelete = books.FirstOrDefault(b => b.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (bookToDelete != null)
            {
                books.Remove(bookToDelete);
                return $"Success! '{bookToDelete.BookTitle}' has been removed from the API list.";
            }
            else
            {
                return $"Sorry, '{title}' is not in the API list, so it cannot be deleted.";
            }
        }
        [HttpPut("{title}")]
        public string Put(string title, int newStock)
        {
            var bookToAddStock = books.FirstOrDefault(b => b.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (bookToAddStock != null)
            {
                bookToAddStock.Stock = newStock; // Update the stock of the found book
                return $"Success! '{bookToAddStock.BookTitle}' stock updated to {newStock}.";
            }
            else
            {
                return $"Sorry, '{title}' is not in the API list, so its stock cannot be updated.";
            }
        }
        [HttpDelete("{title}/passcode")]
        public string DeleteWithPasscode(string title, string passcode)
        {
            if (passcode != "admin123")
            {
                return $"Unauthorized: Incorrect passcode. '{title}' cannot be deleted.";
            }
            var bookToDelete = books.FirstOrDefault(b => b.BookTitle.Equals(title, StringComparison.OrdinalIgnoreCase));
                
            if (bookToDelete != null)
            {
                books.Remove(bookToDelete);
                return $"Success! '{bookToDelete.BookTitle}' has been removed from the API list.";
            }
            else
            {
                return $"Sorry, '{title}' is not in the API list, so it cannot be deleted.";
            }
        }
            
    }

}