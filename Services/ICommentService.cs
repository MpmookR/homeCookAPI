using homeCookAPI.Models;

public interface ICommentService
{
    // Task<IEnumerable<CommentDTO>> GetAllCommentsAsync();
    // Task<IEnumerable<CommentDTO>> GetCommentRepliesAsync(int commentId);
    Task<CommentDTO> UpdateCommentAsync(int commentId, string userId, string newContent);
    Task<CommentDTO> AddCommentAsync(string userId, CommentDTO commentDTO);
    Task<bool> DeleteCommentAsync(int commentId, string userId);
}
