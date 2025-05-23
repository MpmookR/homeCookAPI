using Microsoft.AspNetCore.Mvc;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedRecipesController : ControllerBase
    {
        private readonly ISavedRecipeService _savedRecipeService;
        private readonly ILogger<SavedRecipesController> _logger;

        public SavedRecipesController(ISavedRecipeService savedRecipeService, ILogger<SavedRecipesController> logger)
        {
            _savedRecipeService = savedRecipeService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all saved recipes for a specific user
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>A list of saved recipes</returns>
        /// <response code="200">Returns the list of saved recipes</response>
        /// <response code="403">Unauthorized access</response>
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SavedRecipeDTO>>> GetSavedRecipesByUser(string userId)
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != loggedInUserId)
            {
                _logger.LogWarning("Unauthorized access: User {UserId} attempted to view another user's saved recipes.", loggedInUserId);
                return Forbid("You are not authorized to view other users' saved recipes.");
            }

            _logger.LogInformation("Fetching saved recipes for user {UserId}", userId);
            var savedRecipes = await _savedRecipeService.GetSavedRecipesByUserAsync(userId);
            return Ok(savedRecipes);
        }

        /// <summary>
        /// Saves a recipe for the logged-in user
        /// </summary>
        /// <param name="request">The recipe ID to save</param>
        /// <returns>The saved recipe details</returns>
        /// <response code="200">Recipe saved successfully</response>
        /// <response code="400">Invalid request</response>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<SavedRecipeDTO>> PostSavedRecipe([FromBody] SavedRecipeDTO request)
        {
            // Log the incoming request data
            // _logger.LogInformation("Incoming request body: RecipeId = {RecipeId}", request.RecipeId);

            if (request.RecipeId == 0)
            {
                // _logger.LogWarning("RecipeId is missing or 0!");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //Get UserId from authentication
            // _logger.LogInformation("User {UserId} is saving recipe {RecipeId}", userId, request.RecipeId);

            try
            {
                var savedRecipe = await _savedRecipeService.SaveRecipeAsync(userId, request.RecipeId);
                return Ok(new { message = "Recipe successfully saved!", savedRecipe });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Removes a saved recipe for the logged-in user
        /// </summary>
        /// <param name="savedRecipeId">The ID of the saved recipe.</param>
        /// <returns>A confirmation message if the recipe was successfully removed</returns>
        /// <response code="200">Recipe removed successfully</response>
        /// <response code="400">Invalid request</response>
        [Authorize]
        [HttpDelete("{savedRecipeId}")]
        public async Task<IActionResult> DeleteSavedRecipe(int savedRecipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} is attempting to unsave recipe {SavedRecipeId}", userId, savedRecipeId);

            try
            {
                await _savedRecipeService.RemoveSavedRecipeAsync(savedRecipeId, userId);
                return Ok(new { message = "Saved recipe successfully removed!" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
