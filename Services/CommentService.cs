using homeCookAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public async Task<CommentDTO> UpdateCommentAsync(int commentId, string userId, string newContent)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException($"Comment with ID {commentId} not found.");

        //the user can only edit their own comment
        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to update this comment.");

        comment.Content = newContent;
        await _commentRepository.UpdateAsync(comment);

        return MapToDTO(comment);
    }

    public async Task<CommentDTO> AddCommentAsync(string userId, CommentDTO commentDTO)
    {
        if (!await _recipeRepository.ExistsAsync(commentDTO.RecipeId))
            throw new KeyNotFoundException($"Recipe with ID {commentDTO.RecipeId} not found.");

        if (commentDTO.ParentCommentId.HasValue && !await _commentRepository.ExistsAsync(commentDTO.ParentCommentId.Value))
            throw new KeyNotFoundException($"Parent comment with ID {commentDTO.ParentCommentId} not found.");

        var comment = new Comment
        {
            Content = commentDTO.Content,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            RecipeId = commentDTO.RecipeId,
            ParentCommentId = commentDTO.ParentCommentId
        };

        await _commentRepository.AddAsync(comment);
        return MapToDTO(comment);
    }

    public async Task<bool> DeleteCommentAsync(int commentId, string userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException($"Comment with ID {commentId} not found.");

        // Ensure the user can only delete their own comment
        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to delete this comment.");

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
            RecipeId = comment.RecipeId,
            ParentCommentId = comment.ParentCommentId
        };
    }
}
