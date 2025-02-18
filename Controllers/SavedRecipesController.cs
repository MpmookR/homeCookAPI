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
        [HttpGet("{id}")]
        public async Task<ActionResult<SavedRecipe>> GetSavedRecipe(int id)
        {
            var savedRecipe = await _context.SavedRecipes.FindAsync(id);

            if (savedRecipe == null)
            {
                return NotFound();
            }

            return savedRecipe;
        }

        // PUT: api/SavedRecipes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSavedRecipe(int id, SavedRecipe savedRecipe)
        {
            if (id != savedRecipe.Id)
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
                if (!SavedRecipeExists(id))
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SavedRecipe>> PostSavedRecipe(SavedRecipe savedRecipe)
        {
            _context.SavedRecipes.Add(savedRecipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSavedRecipe", new { id = savedRecipe.Id }, savedRecipe);
        }

        // DELETE: api/SavedRecipes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSavedRecipe(int id)
        {
            var savedRecipe = await _context.SavedRecipes.FindAsync(id);
            if (savedRecipe == null)
            {
                return NotFound();
            }

            _context.SavedRecipes.Remove(savedRecipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SavedRecipeExists(int id)
        {
            return _context.SavedRecipes.Any(e => e.Id == id);
        }
    }
}
