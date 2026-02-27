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

        public List<string> CurrentBorrowerLibraryCard { get; set; } = new List<string>();
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
        public string BorrowerLibCard { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}