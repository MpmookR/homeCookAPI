using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // Get saved recipes for a specific user
        // api/savedrecipes/user/{userId}
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

        // Save a recipe
        // api/savedrecipes >> body: "recipeId": 123
        [Authorize] 
        [HttpPost]
        public async Task<ActionResult<SavedRecipeDTO>> PostSavedRecipe([FromBody] int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} is saving recipe {RecipeId}", userId, recipeId);

            try
            {
                var savedRecipe = await _savedRecipeService.SaveRecipeAsync(userId, recipeId);
                return Ok(new { message = "Recipe successfully saved!", savedRecipe });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // Unsave a recipe
        // api/savedrecipes/{savedRecipeId}
        [Authorize] // Only logged-in users can unsave a recipe
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
