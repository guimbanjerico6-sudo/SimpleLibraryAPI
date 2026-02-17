using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SimpleLibraryAPI.Models
{
    public class Book
    {
        //Book model with validation attributes
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string BookTitle { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Author { get; set; }

        [Required]
        [Range(0, 10000)]
        public int Stock { get; set; }

        public int MaxStock { get; set; }

        public List<int> CurrentBorrowerIds { get; set; } = new List<int>();
    }

    public class AddBookRequest
    {
        // AddBookRequest model for POST requests with validation attributes
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string BookTitle { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Author { get; set; }

        [Required]
        [Range(1, 10000)]
        public int Stock { get; set; }
    }

    public class ActivityLog
    {
        // ActivityLog model to track borrowing and returning of books
        public string Action { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int BorrowerId { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    public class User
    {
        // User model to represent library users
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LibraryCard { get; set; } = string.Empty;
    }
    public class AddUserRequest
    {
        // The user ONLY provides their name.
        public string FullName { get; set; }
    }
}