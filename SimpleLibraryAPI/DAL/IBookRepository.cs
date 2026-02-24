using SimpleLibraryAPI.Models;

namespace SimpleLibraryAPI.DAL
{
    public interface IBookRepository 
    {
        // --- READ OPERATIONS ---
        List<Book> GetAllBooks(); 
        List<User> GetAllUsers();
        List<ActivityLog> GetHistory();
        Book? GetBookByTitle(string title);                                                                                                                 
        User? GetUserByCard(string cardNum);

        // --- WRITE OPERATIONS ---
        void SaveChanges();
        void AddBook(Book book);
        void RemoveBook(Book book);
        void AddUser(User user);
        void AddLog(ActivityLog log);
    }
}