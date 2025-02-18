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
    public class RecipesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Recipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRecipes()
        {
            var recipes = await _context.Recipes
                .Include(r => r.User) // Ensure User is loaded
                .Select(r => new
                {
                    r.RecipeId,
                    r.Name,
                    r.Intro,
                    r.Ingredients,
                    r.HowTo,
                    r.Image,
                    r.CreateDate,
                    User = r.User != null ? new { r.User.Id, r.User.FullName } : null, // Return User details
                    r.UserId,
                    r.Comments,
                    r.Ratings,
                    r.Likes,
                    r.SavedRecipes
                })
                .ToListAsync();

            return Ok(recipes);
        }

        // Get a single recipe by ID and include user details
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRecipe(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.User) // Load User details
                .Where(r => r.RecipeId == id)
                .Select(r => new
                {
                    r.RecipeId,
                    r.Name,
                    r.Intro,
                    r.Ingredients,
                    r.HowTo,
                    r.Image,
                    r.CreateDate,
                    User = r.User != null ? new { r.User.Id, r.User.FullName } : null,
                    r.UserId,
                    r.Comments,
                    r.Ratings,
                    r.Likes,
                    r.SavedRecipes
                })
                .FirstOrDefaultAsync();

            if (recipe == null)
            {
                return NotFound(new { message = "Recipe not found" });
            }

            return Ok(recipe);
        }

        // PUT: api/Recipes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(int id, [FromBody] Recipe recipeUpdate)
        {
            var existingRecipe = await _context.Recipes.FindAsync(id);

            if (existingRecipe == null)
            {
                return NotFound(new { message = "Recipe not found" });
            }

            // Only update provided fields
            existingRecipe.Name = recipeUpdate.Name ?? existingRecipe.Name;
            existingRecipe.Intro = recipeUpdate.Intro ?? existingRecipe.Intro;
            existingRecipe.Ingredients = recipeUpdate.Ingredients ?? existingRecipe.Ingredients;
            existingRecipe.HowTo = recipeUpdate.HowTo ?? existingRecipe.HowTo;
            existingRecipe.Image = recipeUpdate.Image ?? existingRecipe.Image;

            // Mark the entity as modified so EF Core knows it has changed
            _context.Entry(existingRecipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecipeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Recipe updated successfully!", recipe = existingRecipe });
        }


        // POST: api/Recipes
        [HttpPost]
        public async Task<ActionResult<Recipe>> PostRecipe(Recipe recipe)
        {
            if (string.IsNullOrEmpty(recipe.UserId)) // Ensure UserId is provided
                return BadRequest(new { message = "UserId is required." });

            // Set the creation date automatically
            recipe.CreateDate = DateTime.UtcNow;

            // Do NOT manually assign `User`, EF Core will link it using `UserId`
            recipe.User = null;

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecipe", new { id = recipe.RecipeId }, recipe);
        }

        // DELETE: api/Recipes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound(new { message = "Recipe not found" });
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Recipe has been deleted successfully!" });
        }

        private bool RecipeExists(int id)
        {
            return _context.Recipes.Any(e => e.RecipeId == id);
        }
    }
}
