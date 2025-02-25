using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // api/Comments 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments()
        {
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

            return Ok(comments);
        }

        // api/comments/{commentId}/replies 
        [HttpGet("{commentId}/replies")]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetCommentReplies(int commentId)
        {
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
                return NotFound(new { message = "No replies found for this comment" });
            }

            return Ok(replies);
        }

        // api/Comments/{commentId} 
        [HttpPut("{commentId}")]
        public async Task<IActionResult> PutComment(int commentId, CommentDTO commentUpdate)
        {
            var existingComment = await _context.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (existingComment == null)
            {
                return NotFound(new { message = "Comment not found" });
            }

            //Update only the content
            existingComment.Content = commentUpdate.Content ?? existingComment.Content;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Comments.Any(c => c.CommentId == commentId))
                {
                    return NotFound(new { message = "Comment not found." });
                }
                else
                {
                    throw;
                }
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
            // Ensure the Recipe exists
            var recipe = await _context.Recipes.FindAsync(commentDTO.RecipeId);
            if (recipe == null)
            {
                return BadRequest(new { message = "Recipe not found" });
            }

            // Ensure the User exists
            var user = await _context.Users.FindAsync(commentDTO.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Ensure Parent Comment exists if it's a reply
            if (commentDTO.ParentCommentId != null)
            {
                var parentComment = await _context.Comments.FindAsync(commentDTO.ParentCommentId);
                if (parentComment == null)
                {
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
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound(new { message = "Comment not found" });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment has been successfully deleted!" });
        }

        // Check if a comment exists
        private bool CommentExists(int commentId)
        {
            return _context.Comments.Any(e => e.CommentId == commentId);
        }
    }
}
