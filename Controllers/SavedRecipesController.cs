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

        // GET: api/SavedRecipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SavedRecipe>>> GetSavedRecipes()
        {
            return await _context.SavedRecipes.ToListAsync();
        }

        // GET: api/SavedRecipes/5
        [HttpGet("{savedRecipeId}")]
        public async Task<ActionResult<SavedRecipe>> GetSavedRecipe(int savedRecipeId)
        {
            var savedRecipe = await _context.SavedRecipes.FindAsync(savedRecipeId);
            if (savedRecipe == null)
            {
                return NotFound();
            }

            return savedRecipe;
        }

        // PUT: api/SavedRecipes/5
        [HttpPut("{savedRecipeId}")]
        public async Task<IActionResult> PutSavedRecipe(int savedRecipeId, SavedRecipe savedRecipe)
        {
            if (savedRecipeId != savedRecipe.SavedRecipeId) // ✅ Updated from Id to SavedRecipeId
            {
                return BadRequest();
            }

            _context.Entry(savedRecipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SavedRecipeExists(savedRecipeId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SavedRecipes
        [HttpPost]
        public async Task<ActionResult<SavedRecipe>> PostSavedRecipe(SavedRecipe savedRecipe)
        {
            _context.SavedRecipes.Add(savedRecipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSavedRecipe", new { savedRecipeId = savedRecipe.SavedRecipeId }, savedRecipe);
        }

        // DELETE: api/SavedRecipes/5
        [HttpDelete("{savedRecipeId}")]
        public async Task<IActionResult> DeleteSavedRecipe(int savedRecipeId)
        {
            var savedRecipe = await _context.SavedRecipes.FindAsync(savedRecipeId);
            if (savedRecipe == null)
            {
                return NotFound();
            }

            _context.SavedRecipes.Remove(savedRecipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SavedRecipeExists(int savedRecipeId)
        {
            return _context.SavedRecipes.Any(e => e.SavedRecipeId == savedRecipeId);
        }
    }
}
