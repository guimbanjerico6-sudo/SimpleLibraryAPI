using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace SimpleLibraryAPI.Models
{
    public class Book
    {
        [Required]
        [StringLength(100, MinimumLength =3)]
        public string BookTitle { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Author { get; set; }
        [Required]
        [Range(1, 10000)]
        public int Stock { get; set; }  
        [JsonIgnore]
        public int MaxStock { get; set; }
        [JsonIgnore]
        public List<int> CurrentBorrowerIds { get; set; } = new List<int>();
    }
    public class ActivityLog
    {
        public string Action { get; set; } = string.Empty; // Add, Delete, Borrow, etc.
        public string BookTitle { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } // When it happened
        public int BorrowerId { get; set; }
        public string Details { get; set; } = string.Empty; // e.g., "Admin added 5 copies"
    }
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LibraryCard { get; set; } = string.Empty;
    }
}
