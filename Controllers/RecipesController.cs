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

        /// <summary>
        /// Retrieves all recipes, showing related entitied(Like, comment, save, rating)
        /// </summary>
        /// <returns>A list of all recipes in the system.</returns>
        /// <response code="200">Returns the list of recipes</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeDTO>>> GetRecipes()
        {
            _logger.LogInformation("Fetching all recipes...");
            var recipes = await _recipeService.GetAllRecipesAsync();

            _logger.LogInformation("Successfully retrieved {Count} recipes.", recipes.Count());
            return Ok(recipes); //HTTP 200
        }

        /// <summary>
        /// Retrieves a specific recipe by ID.
        /// </summary>
        /// <param name="id">The unique identifier of the recipe.</param>
        /// <returns>The details of the requested recipe.</returns>
        /// <response code="200">Returns the requested recipe</response>
        /// <response code="404">Recipe not found</response>        
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

        /// <summary>
        /// Updates an existing recipe.
        /// </summary>
        /// <param name="id">The unique identifier of the recipe to be updated.</param>
        /// <param name="recipeDTO">The updated recipe data.</param>
        /// <returns>The updated recipe details.</returns>
        /// <response code="200">Recipe updated successfully</response>
        /// <response code="404">Recipe not found</response>
        /// <response code="403">Unauthorized access</response>
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

        /// <summary>
        /// Creates a new recipe.
        /// </summary>
        /// <param name="recipeDTO">The recipe data to be created.</param>
        /// <returns>The newly created recipe.</returns>
        /// <response code="201">Recipe created successfully</response>
        /// <response code="400">Invalid request data</response>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RecipeDTO>> PostRecipe(RecipeDTO recipeDTO)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // From token!
            _logger.LogInformation("Creating a new recipe for User ID: {UserId}", recipeDTO.UserId);
            try
            {
                recipeDTO.UserId = userId; // Override whatever was sent
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

        /// <summary>
        /// Deletes a recipe by ID.
        /// </summary>
        /// <param name="recipeId">The unique identifier of the recipe to be deleted.</param>
        /// <returns>A message indicating whether the deletion was successful.</returns>
        /// <response code="200">Recipe deleted successfully</response>
        /// <response code="404">Recipe not found</response>
        /// <response code="403">Unauthorized access</response>
        [Authorize] // Any logged-in user can attempt this action
        [HttpDelete("{recipeId}")]
        public async Task<IActionResult> DeleteRecipe(int recipeId)
        {
            _logger.LogInformation("Attempting to delete recipe with ID: {RecipeId}", recipeId);

            // Extract user ID from token (mapped via ClaimTypes.NameIdentifier)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No UserId found in token.");
                return Unauthorized(new { message = "User ID not found in token." });
            }

            // Check if user has Admin or SuperAdmin role
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            _logger.LogInformation("👤 User ID: {UserId}, Roles: [{Roles}]", userId, string.Join(", ", roles));

            try
            {
                await _recipeService.DeleteRecipeAsync(recipeId, userId, isAdmin);

                _logger.LogInformation("Recipe {RecipeId} deleted by user {UserId}", recipeId, userId);
                return Ok(new { message = "Recipe deleted successfully!" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Recipe not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized delete attempt by {UserId}: {Message}", userId, ex.Message);
                return StatusCode(403, new { message = ex.Message });
            }
        }
    }
}
