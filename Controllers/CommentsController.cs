using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using Microsoft.Extensions.Logging;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommentsController> _logger;


        public CommentsController(ApplicationDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;

        }

        // api/Comments 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments()
        {
            _logger.LogInformation("Fetching all comments from the database...");

            var comments = await _context.Comments
                .Include(c => c.User)
                .Select(c => new CommentDTO
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    UserName = c.User.FullName,
                    RecipeId = c.RecipeId
                })
                .ToListAsync();
           
            _logger.LogInformation("Successfully retrieved {Count} comments.", comments.Count);
            return Ok(comments);
        }

        // api/comments/{commentId}/replies 
        [HttpGet("{commentId}/replies")]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetCommentReplies(int commentId)
        {
            _logger.LogInformation("Fetching replies for Comment ID {CommentId}", commentId);

            var replies = await _context.Comments
                .Where(c => c.ParentCommentId == commentId)
                .Include(c => c.User)
                .Select(c => new CommentDTO
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    UserName = c.User.FullName,
                    RecipeId = c.RecipeId
                })
                .ToListAsync();

            if (!replies.Any())
            {
                _logger.LogWarning("No replies found for Comment ID {CommentId}", commentId);
                return NotFound(new { message = "No replies found for this comment" });
            }
            
            _logger.LogInformation("Successfully retrieved {Count} replies for Comment ID {CommentId}", replies.Count, commentId);
            return Ok(replies);
        }

        // api/Comments/{commentId} 
        [HttpPut("{commentId}")]
        public async Task<IActionResult> PutComment(int commentId, CommentDTO commentUpdate)
        {
            _logger.LogInformation("Updating Comment ID {CommentId}", commentId);

            var existingComment = await _context.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (existingComment == null)
            {
                _logger.LogWarning("Failed to update - Comment ID {CommentId} not found.", commentId);
                return NotFound(new { message = "Comment not found" });
            }

            //Update only the content
            existingComment.Content = commentUpdate.Content ?? existingComment.Content;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Comment ID {CommentId} updated successfully.", commentId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating Comment ID {CommentId}", commentId);
                throw;
            }

            // Return structured `CommentDTO`
            return Ok(new
            {
                message = "Comment has been successfully updated!",
                comment = new CommentDTO
                {
                    CommentId = existingComment.CommentId,
                    Content = existingComment.Content,
                    CreatedAt = existingComment.CreatedAt, // Keeps original timestamp
                    UserId = existingComment.UserId,
                    UserName = existingComment.User?.FullName,
                    RecipeId = existingComment.RecipeId
                }
            });
        }

        //api/Comments 
        [HttpPost]
        public async Task<ActionResult<CommentDTO>> PostComment(CommentDTO commentDTO)
        {
            _logger.LogInformation("User {UserId} is adding a comment to Recipe ID {RecipeId}.", commentDTO.UserId, commentDTO.RecipeId);

            // Ensure the Recipe exists
            var recipe = await _context.Recipes.FindAsync(commentDTO.RecipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Failed to add comment - Recipe ID {RecipeId} not found.", commentDTO.RecipeId);
                return BadRequest(new { message = "Recipe not found" });
            }

            // Ensure the User exists
            var user = await _context.Users.FindAsync(commentDTO.UserId);
            if (user == null)
            {
                _logger.LogWarning("Failed to add comment - User ID {UserId} not found.", commentDTO.UserId);
                return BadRequest(new { message = "User not found" });
            }

            // Ensure Parent Comment exists if it's a reply
            if (commentDTO.ParentCommentId != null)
            {
                var parentComment = await _context.Comments.FindAsync(commentDTO.ParentCommentId);
                if (parentComment == null)
                {
                    _logger.LogWarning("Failed to add reply - Parent Comment ID {ParentCommentId} not found.", commentDTO.ParentCommentId);
                    return BadRequest(new { message = "Parent comment not found" });
                }
            }

            // Convert `CommentDTO` to `Comment` before saving
            var comment = new Comment
            {
                Content = commentDTO.Content,
                CreatedAt = DateTime.UtcNow,
                UserId = commentDTO.UserId,
                RecipeId = commentDTO.RecipeId,
                ParentCommentId = commentDTO.ParentCommentId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comment ID {CommentId} added successfully.", comment.CommentId);
            // Return structured `CommentDTO`
            return Ok(new
            {
                message = "Comment has been successfully added!",
                comment = new CommentDTO
                {
                    CommentId = comment.CommentId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    UserId = comment.UserId,
                    UserName = user.FullName, //Return UserName instead of full user's details
                    RecipeId = comment.RecipeId
                }
            });
        }

        // api/Comments/{commentId} 
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            _logger.LogInformation("Attempting to delete Comment ID {CommentId}.", commentId);

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning("Failed to delete - Comment ID {CommentId} not found.", commentId);
                return NotFound(new { message = "Comment not found" });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comment ID {CommentId} deleted successfully.", commentId);
            return Ok(new { message = "Comment has been successfully deleted!" });
        }

        // Check if a comment exists
        private bool CommentExists(int commentId)
        {
            return _context.Comments.Any(e => e.CommentId == commentId);
        }
    }
}
