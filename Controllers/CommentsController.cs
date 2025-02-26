using Microsoft.AspNetCore.Mvc;
using homeCookAPI.Models;

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

        // api/Comments 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments()
        {
            _logger.LogInformation("Fetching all comments...");
            var comments = await _commentService.GetAllCommentsAsync();
            _logger.LogInformation("Successfully retrieved {Count} comments.", comments.Count());
            return Ok(comments);
        }

        // api/comments/{commentId}/replies 
        [HttpGet("{commentId}/replies")]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetCommentReplies(int commentId)
        {
            _logger.LogInformation("Fetching replies for Comment ID {CommentId}", commentId);

            try
            {
                var replies = await _commentService.GetCommentRepliesAsync(commentId);

                if (!replies.Any())
                {
                    _logger.LogWarning("No replies found for Comment ID {CommentId}", commentId);
                    return NotFound(new { message = "No replies found for this comment" });
                }

                _logger.LogInformation("Successfully retrieved {Count} replies for Comment ID {CommentId}", replies.Count(), commentId);
                return Ok(replies);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        // api/Comments/{commentId} 
        [HttpPut("{commentId}")]
        public async Task<IActionResult> PutComment(int commentId, CommentDTO commentUpdate)
        {
            _logger.LogInformation("Updating Comment ID {CommentId}", commentId);

            try
            {
                var updatedComment = await _commentService.UpdateCommentAsync(commentId, commentUpdate.Content);

                _logger.LogInformation("Comment ID {CommentId} updated successfully.", commentId);
                return Ok(new
                {
                    message = "Comment has been successfully updated!",
                    comment = updatedComment
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        //api/Comments 
        [HttpPost]
        public async Task<ActionResult<CommentDTO>> PostComment(CommentDTO commentDTO)
        {
            _logger.LogInformation("User {UserId} is adding a comment to Recipe ID {RecipeId}.", commentDTO.UserId, commentDTO.RecipeId);

            try
            {
                var newComment = await _commentService.AddCommentAsync(commentDTO);

                _logger.LogInformation("Comment ID {CommentId} added successfully.", newComment.CommentId);
                return Ok(new
                {
                    message = "Comment has been successfully added!",
                    comment = newComment
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }


        // api/Comments/{commentId} 
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            _logger.LogInformation("Attempting to delete Comment ID {CommentId}.", commentId);

            try
            {
                await _commentService.DeleteCommentAsync(commentId);

                _logger.LogInformation("Comment ID {CommentId} deleted successfully.", commentId);
                return Ok(new { message = "Comment has been successfully deleted!" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
