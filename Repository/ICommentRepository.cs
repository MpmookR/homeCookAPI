using homeCookAPI.Models;

public interface ICommentRepository
{
    //base on CRUD
    Task<IEnumerable<Comment>> GetAllAsync();
    Task<IEnumerable<Comment>> GetRepliesAsync(int commentId);
    Task<Comment> GetByIdAsync(int commentId);
    Task<bool> ExistsAsync(int commentId);
    Task AddAsync(Comment comment);
    Task UpdateAsync(Comment comment);
    Task DeleteAsync(Comment comment);
}
