namespace SimpleLibraryAPI.Models
{
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
