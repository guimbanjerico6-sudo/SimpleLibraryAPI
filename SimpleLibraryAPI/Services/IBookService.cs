using SimpleLibraryAPI.Models;

namespace SimpleLibraryAPI.Services
{
    public interface IBookService
    {
        // --- READ OPERATIONS ---
        List<Book> GetBooks(); 
        List<User> GetAllUsers();
        List<ActivityLog> GetHistory();
        Book? GetByTitle(string title);
        List<Book> GetByAuthor(string author);

        // --- WRITE OPERATIONS ---
        void AddBook(Book newBook);
        Book? AdminExpansion(string title, int amountToAdd);
        bool DeleteBook(string title);
        void BorrowBook(string title, string userName, string cardNum);
        void ReturnBook(string title, int userId);
        string AddUser(string name);
    }
}