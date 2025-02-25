using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedRecipesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SavedRecipesController> _logger;

        public SavedRecipesController(ApplicationDbContext context, ILogger<SavedRecipesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get saved recipes for a specific user
        // api/savedrecipes/user/{userId}
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SavedRecipeDTO>>> GetSavedRecipesByUser(string userId)
        {
            // Get the logged-in user's ID
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ensure users can only fetch their own saved recipes
            if (userId != loggedInUserId)
            {
                _logger.LogWarning("Unauthorized access: User {UserId} attempted to view another user's saved recipes.", loggedInUserId);
                return Forbid("You are not authorized to view other users' saved recipes.");
            }

            _logger.LogInformation("Fetching saved recipes for user {UserId}", userId);

            var savedRecipes = await _context.SavedRecipes
                .Where(s => s.UserId == userId)
                .Include(s => s.Recipe)
                .Select(s => new SavedRecipeDTO
                {
                    SavedRecipeId = s.SavedRecipeId,
                    UserId = s.UserId,
                    UserName = s.User.FullName,
                    RecipeId = s.RecipeId,
                    RecipeName = s.Recipe.Name,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            if (!savedRecipes.Any())
            {
                _logger.LogWarning("No saved recipes found for user {UserId}", userId);
                return NotFound(new { message = "No saved recipes found." });
            }

            _logger.LogInformation("Successfully retrieved {Count} saved recipes for user {UserId}", savedRecipes.Count, userId);
            return Ok(savedRecipes);
        }

        // Save a recipe
        // api/savedrecipes
        [Authorize] 
        [HttpPost]
        public async Task<ActionResult<object>> PostSavedRecipe(SavedRecipe savedRecipe)
        {
            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            savedRecipe.UserId = userId;

            _logger.LogInformation("User {UserId} is saving recipe {RecipeId}", userId, savedRecipe.RecipeId);

            // Ensure Recipe exists
            var recipe = await _context.Recipes.FindAsync(savedRecipe.RecipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Save failed: Recipe {RecipeId} not found", savedRecipe.RecipeId);
                return BadRequest(new { message = "Recipe not found." });
            }

            // Prevent duplicate saved recipes by the same user
            var existingSavedRecipe = await _context.SavedRecipes
                .FirstOrDefaultAsync(s => s.RecipeId == savedRecipe.RecipeId && s.UserId == userId);
            if (existingSavedRecipe != null)
            {
                _logger.LogWarning("User {UserId} has already saved recipe {RecipeId}", userId, savedRecipe.RecipeId);
                return BadRequest(new { message = "Recipe is already saved by this user." });
            }

            // Save the recipe
            savedRecipe.CreatedAt = DateTime.UtcNow;
            _context.SavedRecipes.Add(savedRecipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} successfully saved recipe {RecipeId}", userId, savedRecipe.RecipeId);

            return Ok(new
            {
                message = "Recipe successfully saved!",
                savedRecipe = new
                {
                    savedRecipe.SavedRecipeId,
                    savedRecipe.CreatedAt,
                    savedRecipe.UserId,
                    savedRecipe.RecipeId
                }
            });
        }

        // Unsave a recipe
        // api/savedrecipes/{savedRecipeId}
        [Authorize] // Only logged-in users can unsave a recipe
        [HttpDelete("{savedRecipeId}")]
        public async Task<IActionResult> DeleteSavedRecipe(int savedRecipeId)
        {
            var savedRecipe = await _context.SavedRecipes.FindAsync(savedRecipeId);
            if (savedRecipe == null)
            {
                _logger.LogWarning("Unsave failed: SavedRecipe {SavedRecipeId} not found", savedRecipeId);
                return NotFound(new { message = "Saved recipe not found." });
            }

            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ensure users can only delete their own saved recipes
            if (savedRecipe.UserId != userId)
            {
                _logger.LogWarning("Unauthorized access: User {UserId} attempted to unsave another user's recipe.", userId);
                return Forbid("You are not authorized to unsave this recipe.");
            }

            _context.SavedRecipes.Remove(savedRecipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} successfully unsaved recipe {RecipeId}", userId, savedRecipe.RecipeId);

            return Ok(new { message = "Saved recipe has been successfully removed!" });
        }
    }
}
