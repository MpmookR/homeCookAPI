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

        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetComments()
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Recipe)
                .Include(c => c.Replies)
                .Select(c => new
                {
                    c.CommentId,
                    c.Content,
                    c.CreatedAt,
                    User = c.User != null ? new { c.User.Id, c.User.FullName } : null,
                    Recipe = c.Recipe != null ? new { c.RecipeId, c.Recipe.Name } : null,
                    c.ParentCommentId,
                    Replies = c.Replies.Select(r => new
                    {
                        r.CommentId,
                        r.Content,
                        r.CreatedAt,
                        User = r.User != null ? new { r.User.Id, r.User.FullName } : null
                    }).ToList()
                })
                .ToListAsync();

            return Ok(comments);
        }


        // GET: api/Comments/5
        [HttpGet("{commentId}")]
        public async Task<ActionResult<Comment>> GetComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }

        // GET: api/comments/1/replies
        [HttpGet("{commentId}/replies")]
        public async Task<ActionResult<IEnumerable<object>>> GetCommentReplies(int commentId)
        {
            var replies = await _context.Comments
                .Where(c => c.ParentCommentId == commentId)
                .Include(c => c.User)
                .Select(c => new
                {
                    c.CommentId,
                    c.Content,
                    c.CreatedAt,
                    User = c.User != null ? new { c.User.Id, c.User.FullName } : null,
                    c.ParentCommentId
                })
                .ToListAsync();

            if (replies == null || replies.Count == 0)
            {
                return NotFound(new { message = "No replies found for this comment" });
            }

            return Ok(replies);
        }


        // PUT: api/Comments/5
        [HttpPut("{commentId}")]
        public async Task<IActionResult> PutComment(int commentId, Comment commentUpdate)
        {
            var existingComment = await _context.Comments.FindAsync(commentId);

            if (existingComment == null)
            {
                return NotFound(new { message = "Comment not found" });
            }

            existingComment.Content = commentUpdate.Content ?? existingComment.Content;
            existingComment.CreatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Comments.Any(c => c.CommentId == commentId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new
            {
                message = "Comment has been successfully updated!",
                comment = new
                {
                    existingComment.CommentId,
                    existingComment.Content,
                    existingComment.CreatedAt,
                    existingComment.UserId,
                    existingComment.RecipeId,
                    existingComment.ParentCommentId
                }
            });
        }


        // POST: api/Comments
        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(Comment comment)
        {
            // Ensure the Recipe exists
            var recipe = await _context.Recipes.FindAsync(comment.RecipeId);
            if (recipe == null)
            {
                return BadRequest(new { message = "Recipe not found" });
            }

            // Ensure the User exists
            var user = await _context.Users.FindAsync(comment.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Ensure Parent Comment exists if it's a reply
            if (comment.ParentCommentId != null)
            {
                var parentComment = await _context.Comments.FindAsync(comment.ParentCommentId);
                if (parentComment == null)
                {
                    return BadRequest(new { message = "Parent comment not found" });
                }
            }

            // Set CreatedAt automatically
            comment.CreatedAt = DateTime.UtcNow;

            // Prevent EF from expecting full objects
            comment.User = null;
            comment.Recipe = null;
            comment.ParentComment = null;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment has been successfully added!",
                comment = new
                {
                    comment.CommentId,
                    comment.Content,
                    comment.CreatedAt,
                    comment.UserId,
                    comment.RecipeId,
                    comment.ParentCommentId
                }
            });
        }

        // DELETE: api/Comments/5
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


        private bool CommentExists(int commentId)
        {
            return _context.Comments.Any(e => e.CommentId == commentId);
        }

    }
}
