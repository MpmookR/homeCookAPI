using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(IRecipeService recipeService, ILogger<RecipesController> logger)
        {
            _recipeService = recipeService;
            _logger = logger;
        }
        // GET: api/Recipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeDTO>>> GetRecipes()
        {
            _logger.LogInformation("Fetching all recipes...");
            var recipes = await _recipeService.GetAllRecipesAsync();

            _logger.LogInformation("Successfully retrieved {Count} recipes.", recipes.Count());
            return Ok(recipes);
        }

        // Get a single recipe by ID and include user details
        [HttpGet("{id}")]    
        public async Task<ActionResult<RecipeDTO>> GetRecipe(int id)
        {
            _logger.LogInformation("Fetching recipe with ID: {RecipeId}", id);
            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);
                _logger.LogInformation("Successfully retrieved recipe with ID: {RecipeId}", id);
                return Ok(recipe);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/Recipes/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(int id, RecipeDTO recipeDTO)
        {
            _logger.LogInformation("Updating recipe with ID: {RecipeId}", id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            try
            {
                var updatedRecipe = await _recipeService.UpdateRecipeAsync(id, recipeDTO, userId);
                _logger.LogInformation("Recipe updated successfully with ID: {RecipeId} by User ID: {UserId}", id, userId);
                return Ok(new { message = "Recipe updated successfully!", recipe = updatedRecipe });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid(ex.Message); 
            }
        }

        // POST: api/Recipes
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RecipeDTO>> PostRecipe(RecipeDTO recipeDTO)
        {
            _logger.LogInformation("Creating a new recipe for User ID: {UserId}", recipeDTO.UserId);
            try
            {
                var newRecipe = await _recipeService.AddRecipeAsync(recipeDTO);
                _logger.LogInformation("Recipe created successfully with ID: {RecipeId}", newRecipe.RecipeId);
                return CreatedAtAction(nameof(GetRecipe), new { id = newRecipe.RecipeId }, newRecipe);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Recipes/5
        [Authorize] // Any logged-in user can attempt this action
        [HttpDelete("{recipeId}")]
        public async Task<IActionResult> DeleteRecipe(int RecipeId)
            {
                _logger.LogInformation("Attempting to delete recipe with ID: {RecipeId}", RecipeId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

                try
                {
                    await _recipeService.DeleteRecipeAsync(RecipeId, userId, isAdmin);
                    _logger.LogInformation("Recipe deleted successfully with ID: {RecipeId} by User ID: {UserId}", RecipeId, userId);
                    return Ok(new { message = "Recipe deleted successfully!" });
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogWarning(ex.Message);
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex.Message);
                    return Forbid(ex.Message); 

                }
            }
    }
}
