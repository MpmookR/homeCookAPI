using homeCookAPI.Models;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRecipeRepository _recipeRepository;

    public CommentService(ICommentRepository commentRepository, IUserRepository userRepository, IRecipeRepository recipeRepository)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<IEnumerable<CommentDTO>> GetAllCommentsAsync()
    {
        var comments = await _commentRepository.GetAllAsync();
        return comments.Select(MapToDTO);
    }

    public async Task<IEnumerable<CommentDTO>> GetCommentRepliesAsync(int commentId)
    {
        var replies = await _commentRepository.GetRepliesAsync(commentId);
        return replies.Select(MapToDTO);
    }

    public async Task<CommentDTO> UpdateCommentAsync(int commentId, string newContent)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null) throw new KeyNotFoundException($"Comment with ID {commentId} not found.");

        comment.Content = newContent ?? comment.Content;
        await _commentRepository.UpdateAsync(comment);

        return MapToDTO(comment);
    }

    public async Task<CommentDTO> AddCommentAsync(CommentDTO commentDTO)
    {
        if (!await _recipeRepository.ExistsAsync(commentDTO.RecipeId))
            throw new KeyNotFoundException($"Recipe with ID {commentDTO.RecipeId} not found.");

        if (!await _userRepository.ExistsAsync(commentDTO.UserId))
            throw new KeyNotFoundException($"User with ID {commentDTO.UserId} not found.");

        if (commentDTO.ParentCommentId.HasValue && !await _commentRepository.ExistsAsync(commentDTO.ParentCommentId.Value))
            throw new KeyNotFoundException($"Parent comment with ID {commentDTO.ParentCommentId} not found.");

        var comment = new Comment
        {
            Content = commentDTO.Content,
            CreatedAt = DateTime.UtcNow,
            UserId = commentDTO.UserId,
            RecipeId = commentDTO.RecipeId,
            ParentCommentId = commentDTO.ParentCommentId
        };

        await _commentRepository.AddAsync(comment);
        return MapToDTO(comment);
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null) throw new KeyNotFoundException($"Comment with ID {commentId} not found.");

        await _commentRepository.DeleteAsync(comment);
        return true;
    }

    private static CommentDTO MapToDTO(Comment comment)
    {
        return new CommentDTO
        {
            CommentId = comment.CommentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            UserName = comment.User?.FullName,
            RecipeId = comment.RecipeId
        };
    }
}
