using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedRecipesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SavedRecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // api/savedrecipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SavedRecipeDTO>>> GetSavedRecipes()
        {
            var savedRecipes = await _context.SavedRecipes
                .Include(s => s.User)
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

            return Ok(savedRecipes);
        }


        // GET saved recipe by ID
        [HttpGet("{savedRecipeId}")]
        public async Task<ActionResult<object>> GetSavedRecipe(int savedRecipeId)
        {
            var savedRecipe = await _context.SavedRecipes
                .Include(s => s.User)
                .Include(s => s.Recipe)
                .Where(s => s.SavedRecipeId == savedRecipeId)
                .Select(s => new
                {
                    s.SavedRecipeId,
                    s.CreatedAt,
                    User = s.User != null ? new { s.User.Id, s.User.FullName } : null,
                    Recipe = s.Recipe != null ? new { s.RecipeId, s.Recipe.Name } : null
                })
                .FirstOrDefaultAsync();

            if (savedRecipe == null)
            {
                return NotFound(new { message = "Saved recipe not found" });
            }

            return Ok(savedRecipe);
        }

        // api/savedrecipes
        [HttpPost]
        public async Task<ActionResult<object>> PostSavedRecipe(SavedRecipe savedRecipe)
        {
            // Ensure Recipe exists
            var recipe = await _context.Recipes.FindAsync(savedRecipe.RecipeId);
            if (recipe == null)
            {
                return BadRequest(new { message = "Recipe not found" });
            }

            // Ensure User exists
            var user = await _context.Users.FindAsync(savedRecipe.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Prevent duplicate saved recipes by the same user
            var existingSavedRecipe = await _context.SavedRecipes
                .FirstOrDefaultAsync(s => s.RecipeId == savedRecipe.RecipeId && s.UserId == savedRecipe.UserId);
            if (existingSavedRecipe != null)
            {
                return BadRequest(new { message = "Recipe is already saved by this user" });
            }

            savedRecipe.CreatedAt = DateTime.UtcNow;
            _context.SavedRecipes.Add(savedRecipe);
            await _context.SaveChangesAsync();

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

        //Remove a saved recipe
        [HttpDelete("{savedRecipeId}")]
        public async Task<IActionResult> DeleteSavedRecipe(int savedRecipeId)
        {
            var savedRecipe = await _context.SavedRecipes.FindAsync(savedRecipeId);
            if (savedRecipe == null)
            {
                return NotFound(new { message = "Saved recipe not found" });
            }

            _context.SavedRecipes.Remove(savedRecipe);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Saved recipe has been successfully removed!" });
        }
    }
}
