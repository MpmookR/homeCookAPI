using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        /// <summary>
        /// Retrieves likes for a specific recipe
        /// </summary>
        /// <param name="recipeId">The unique identifier of the recipe</param>
        /// <returns>A list of likes for the specified recipe.</returns>
        /// <response code="200">Returns the list of likes</response>
        /// <response code="404">Recipe not found</response>        
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


        /// <summary>
        /// Likes a recipe
        /// </summary>
        /// <param name="request">The recipe ID to like</param>
        /// <returns>The like details.</returns>
        /// <response code="200">Recipe liked successfully</response>
        /// <response code="400">Invalid request</response>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<LikeDTO>> PostLike([FromBody] LikeDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Extract UserId from authentication

            _logger.LogInformation("User {UserId} is attempting to like Recipe ID {RecipeId}.", userId, request.RecipeId);

            try
            {
                var newLike = await _likeService.AddLikeAsync(userId, request.RecipeId);
                _logger.LogInformation("User {UserId} successfully liked Recipe ID {RecipeId}.", userId, request.RecipeId);
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

        /// <summary>
        /// Unlikes a recipe
        /// </summary>
        /// <param name="recipeId">The ID of the recipe to unlike</param>
        /// <param name="userId">The ID of the user unliking the recipe</param>
        /// <returns>A confirmation message if successful.</returns>
        /// <response code="200">Recipe unliked successfully</response>
        /// <response code="404">Recipe not found</response>
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
