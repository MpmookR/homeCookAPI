using Microsoft.AspNetCore.Mvc;
using homeCookAPI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        // api/Comments/{commentId} 
        [Authorize]
        [HttpPut("{commentId}")]
        public async Task<IActionResult> PutComment(int commentId, [FromBody] CommentDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("User {UserId} is updating Comment ID {CommentId}.", userId, commentId);

            try
            {
                var updatedComment = await _commentService.UpdateCommentAsync(commentId, userId, request.Content);
                _logger.LogInformation("Comment ID {CommentId} updated successfully.", commentId);
                return Ok(new { message = "Comment has been successfully updated!", comment = updatedComment });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
        }



        //api/Comments 
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentDTO>> PostComment([FromBody] CommentDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                _logger.LogWarning("Unauthorized request: Unable to retrieve UserId from authentication.");
                return Unauthorized(new { message = "Unauthorized request: Unable to determine user." });
            }

            _logger.LogInformation("User {UserId} is adding a comment to Recipe ID {RecipeId}.", userId, request.RecipeId);

            try
            {
                var newComment = await _commentService.AddCommentAsync(userId, request);
                _logger.LogInformation("Comment ID {CommentId} added successfully.", newComment.CommentId);
                return Ok(new { message = "Comment has been successfully added!", comment = newComment });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // api/Comments/{commentId} 
        [Authorize]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("User {UserId} is attempting to delete Comment ID {CommentId}.", userId, commentId);

            try
            {
                await _commentService.DeleteCommentAsync(commentId, userId);
                _logger.LogInformation("Comment ID {CommentId} deleted successfully.", commentId);
                return Ok(new { message = "Comment has been successfully deleted!" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
        }

    }
}
