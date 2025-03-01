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

        /// <summary>
        /// Updates an existing comment
        /// </summary>
        /// <param name="commentId">The ID of the comment to update</param>
        /// <param name="request">Updated comment details</param>
        /// <returns>The updated comment</returns>
        /// <response code="200">Comment updated successfully</response>
        /// <response code="403">Unauthorized access</response>
        /// <response code="404">Comment not found</response>
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

        /// <summary>
        /// Adds a new comment to a recipe
        /// </summary>
        /// <param name="request">The comment details</param>
        /// <returns>The newly created comment</returns>
        /// <response code="200">Comment added successfully</response>
        /// <response code="400">Invalid request</response>        
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

        /// <summary>
        /// Deletes a comment
        /// </summary>
        /// <param name="commentId">The ID of the comment to delete.</param>
        /// <returns>A confirmation message if successful</returns>
        /// <response code="200">Comment deleted successfully</response>
        /// <response code="403">Unauthorized access</response>
        /// <response code="404">Comment not found</response>        
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
