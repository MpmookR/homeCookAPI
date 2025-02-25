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
    public class RecipesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(ApplicationDbContext context, ILogger<RecipesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Recipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeDTO>>> GetRecipes()
        {

            _logger.LogInformation("Fetching all recipes...");

            var recipes = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Comments)
                .Include(r => r.Ratings)
                .Include(r => r.Likes)
                .Include(r => r.SavedRecipes)
                .Select(r => new RecipeDTO
                {
                    RecipeId = r.RecipeId,
                    Name = r.Name,
                    Category = r.Category,
                    Intro = r.Intro,
                    Ingredients = r.Ingredients,
                    HowTo = r.HowTo,
                    Image = r.Image,
                    CreateDate = r.CreateDate,

                    // User Details
                    UserId = r.UserId,
                    UserName = r.User.FullName,

                    // Convert Comments to DTOs
                    Comments = r.Comments.Select(c => new CommentDTO
                    {
                        CommentId = c.CommentId,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId,
                        UserName = c.User.FullName,
                        RecipeId = c.RecipeId
                    }).ToList(),

                    // Convert Ratings to DTOs
                    Ratings = r.Ratings.Select(rt => new RecipeRatingDTO
                    {
                        RecipeRatingId = rt.RecipeRatingId,
                        UserId = rt.UserId,
                        UserName = rt.User.FullName,
                        RecipeId = rt.RecipeId,
                        Rating = rt.Rating
                    }).ToList(),

                    // Convert Likes to DTOs
                    Likes = r.Likes.Select(l => new LikeDTO
                    {
                        LikeId = l.LikeId,
                        UserId = l.UserId,
                        UserName = l.User.FullName,
                        RecipeId = l.RecipeId,
                        RecipeName = l.Recipe.Name,
                        CreatedAt = l.CreatedAt
                    }).ToList(),

                    // Convert Saved Recipes to DTOs
                    SavedRecipes = r.SavedRecipes.Select(sr => new SavedRecipeDTO
                    {
                        SavedRecipeId = sr.SavedRecipeId,
                        UserId = sr.UserId,
                        UserName = sr.User.FullName,
                        RecipeId = sr.RecipeId,
                        RecipeName = sr.Recipe.Name,
                        CreatedAt = sr.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} recipes.", recipes.Count);
            return Ok(recipes);
        }

        // Get a single recipe by ID and include user details
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDTO>> GetRecipe(int id)
        {
            _logger.LogInformation("Fetching recipe with ID: {RecipeId}", id);

            var recipe = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Comments)
                .Include(r => r.Ratings)
                .Include(r => r.Likes)
                .Include(r => r.SavedRecipes)
                .Where(r => r.RecipeId == id)
                .Select(r => new RecipeDTO
                {
                    RecipeId = r.RecipeId,
                    Name = r.Name,
                    Category = r.Category,
                    Intro = r.Intro,
                    Ingredients = r.Ingredients,
                    HowTo = r.HowTo,
                    Image = r.Image,
                    CreateDate = r.CreateDate,

                    // User Details
                    UserId = r.UserId,
                    UserName = r.User.FullName,

                    // Convert Comments to DTOs
                    Comments = r.Comments.Select(c => new CommentDTO
                    {
                        CommentId = c.CommentId,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId,
                        UserName = c.User.FullName,
                        RecipeId = c.RecipeId
                    }).ToList(),

                    // Convert Ratings to DTOs
                    Ratings = r.Ratings.Select(rt => new RecipeRatingDTO
                    {
                        RecipeRatingId = rt.RecipeRatingId,
                        UserId = rt.UserId,
                        UserName = rt.User.FullName,
                        RecipeId = rt.RecipeId,
                        Rating = rt.Rating
                    }).ToList(),

                    // Convert Likes to DTOs
                    Likes = r.Likes.Select(l => new LikeDTO
                    {
                        LikeId = l.LikeId,
                        UserId = l.UserId,
                        UserName = l.User.FullName,
                        RecipeId = l.RecipeId,
                        RecipeName = l.Recipe.Name,
                        CreatedAt = l.CreatedAt
                    }).ToList(),

                    // Convert Saved Recipes to DTOs
                    SavedRecipes = r.SavedRecipes.Select(sr => new SavedRecipeDTO
                    {
                        SavedRecipeId = sr.SavedRecipeId,
                        UserId = sr.UserId,
                        UserName = sr.User.FullName,
                        RecipeId = sr.RecipeId,
                        RecipeName = sr.Recipe.Name,
                        CreatedAt = sr.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (recipe == null)
            {
                _logger.LogWarning("Recipe with ID {RecipeId} not found.", id);
                return NotFound(new { message = "Recipe not found" });
            }

            _logger.LogInformation("Successfully retrieved recipe with ID: {RecipeId}", id);
            return Ok(recipe);
        }

        // PUT: api/Recipes/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(int id, [FromBody] Recipe recipeUpdate)
        {
            _logger.LogInformation("Updating recipe with ID: {RecipeId}", id);

            var existingRecipe = await _context.Recipes.FindAsync(id);

            if (existingRecipe == null)
            {
                _logger.LogWarning("Failed to update - Recipe with ID {RecipeId} not found.", id);
                return NotFound(new { message = "Recipe not found" });
            }

            // Get logged-in user ID from JWT token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the logged-in user is the owner of the recipe
            if (existingRecipe.UserId != userId)
            {
                _logger.LogWarning("Unauthorized edit attempt - User {UserId} tried to edit recipe ID {RecipeId}", userId, id);
                return Forbid("You are not authorized to edit this recipe.");
            }

            // Only update provided fields
            existingRecipe.Name = recipeUpdate.Name ?? existingRecipe.Name;
            existingRecipe.Category = recipeUpdate.Category ?? existingRecipe.Category;
            existingRecipe.Intro = recipeUpdate.Intro ?? existingRecipe.Intro;
            existingRecipe.Ingredients = recipeUpdate.Ingredients ?? existingRecipe.Ingredients;
            existingRecipe.HowTo = recipeUpdate.HowTo ?? existingRecipe.HowTo;
            existingRecipe.Image = recipeUpdate.Image ?? existingRecipe.Image;

            // Mark the entity as modified so EF Core knows it has changed
            _context.Entry(existingRecipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Recipe updated successfully with ID: {RecipeId} by User ID: {UserId}", id, userId);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Database concurrency error while updating recipe ID: {RecipeId}", id);
                return BadRequest("Concurrency error occurred.");
            }

            return Ok(new { message = "Recipe updated successfully!", recipe = existingRecipe });
        }

        // POST: api/Recipes
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Recipe>> PostRecipe(Recipe recipe)
        {
            _logger.LogInformation("Creating a new recipe for User ID: {UserId}", recipe.UserId);

            if (string.IsNullOrEmpty(recipe.UserId)) // Ensure UserId is provided
            {
                _logger.LogWarning("Failed to create recipe - UserId is missing.");
                return BadRequest(new { message = "UserId is required." });
            }
            // Set the creation date automatically
            recipe.CreateDate = DateTime.UtcNow;

            // Do NOT manually assign `User`, EF Core will link it using `UserId`
            recipe.User = null;

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Recipe created successfully with ID: {RecipeId}", recipe.RecipeId);
            return CreatedAtAction("GetRecipe", new { id = recipe.RecipeId }, recipe);
        }

        // DELETE: api/Recipes/5
        [Authorize] // Any logged-in user can attempt this action
        [HttpDelete("{recipeId}")]
        public async Task<IActionResult> DeleteRecipe(int recipeId)
        {
            _logger.LogInformation("Attempting to delete recipe with ID: {RecipeId}", recipeId);

            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Failed to delete - Recipe with ID {RecipeId} not found.", recipeId);
                return NotFound(new { message = "Recipe not found." });
            }
            // Get logged-in user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                // If the user is an Admin or SuperAdmin, they can delete any post
                if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                {
                    _context.Recipes.Remove(recipe);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Recipe with ID {RecipeId} deleted by {Role}", recipeId,
                        User.IsInRole("Admin") ? "Admin" : "SuperAdmin");
                    return Ok(new { message = "Recipe deleted successfully by Admin/SuperAdmin." });
                }

                // If the user is the owner of the recipe, they can delete it
                if (recipe.UserId == userId)
                {
                    _context.Recipes.Remove(recipe);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User {UserId} successfully deleted their recipe with ID {RecipeId}", userId, recipeId);
                    return Ok(new { message = "Your recipe has been deleted successfully." });
                }

                // Otherwise, deny access
                _logger.LogWarning("Unauthorized deletion attempt by User {UserId} on Recipe ID {RecipeId}", userId, recipeId);
                return Forbid("You are not authorized to delete this recipe.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while attempting to delete recipe with ID {RecipeId}", recipeId);
                return StatusCode(500, new { message = "An internal error occurred while deleting the recipe." });
            }
        }

        private bool RecipeExists(int id)
        {
            return _context.Recipes.Any(e => e.RecipeId == id);
        }
    }
}
