using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly ILogger<LikesController> _logger;

        public LikesController(ILikeService likeService, ILogger<LikesController> logger)
        {
            _likeService = likeService;
            _logger = logger;
        }

        // api/likes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetLikes()
        {
            _logger.LogInformation("Fetching all likes from the database...");
            var likes = await _likeService.GetAllLikesAsync();
            _logger.LogInformation("Successfully retrieved {Count} likes.", likes.Count());
            return Ok(likes);
        }

        // api/likes/recipe/{recipeId}
        [HttpGet("recipe/{recipeId}")]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetLikesByRecipe(int recipeId)
        {
            _logger.LogInformation("Fetching likes for Recipe ID {RecipeId}.", recipeId);
            try
            {
                var likes = await _likeService.GetLikesByRecipeIdAsync(recipeId);
                _logger.LogInformation("Successfully retrieved {Count} likes for Recipe ID {RecipeId}.", likes.Count(), recipeId);
                return Ok(likes);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }


        // api/likes
        [HttpPost]
        public async Task<ActionResult<LikeDTO>> PostLike(Like like)
        {
            _logger.LogInformation("User {UserId} is attempting to like Recipe ID {RecipeId}.", like.UserId, like.RecipeId);
            try
            {
                var newLike = await _likeService.AddLikeAsync(like);
                _logger.LogInformation("User {UserId} successfully liked Recipe ID {RecipeId}.", like.UserId, like.RecipeId);
                return Ok(new { message = "Recipe successfully liked!", like = newLike });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // api/likes/recipe/{recipeId}/user/{userId}
        [HttpDelete("recipe/{recipeId}/user/{userId}")]
        public async Task<IActionResult> UnlikeRecipe(int recipeId, string userId)
        {
            _logger.LogInformation("User {UserId} is attempting to unlike Recipe ID {RecipeId}.", userId, recipeId);
            try
            {
                await _likeService.UnlikeRecipeAsync(userId, recipeId);
                _logger.LogInformation("User {UserId} successfully unliked Recipe ID {RecipeId}.", userId, recipeId);
                return Ok(new { message = "Recipe successfully unliked!" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
