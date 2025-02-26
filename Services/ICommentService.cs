using homeCookAPI.Models;

public interface ICommentService
{
    Task<IEnumerable<CommentDTO>> GetAllCommentsAsync();
    Task<IEnumerable<CommentDTO>> GetCommentRepliesAsync(int commentId);
    Task<CommentDTO> UpdateCommentAsync(int commentId, string newContent);
    Task<CommentDTO> AddCommentAsync(CommentDTO commentDTO);
    Task<bool> DeleteCommentAsync(int commentId);
}
