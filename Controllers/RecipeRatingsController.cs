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
    public class RecipeRatingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecipeRatingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RecipeRatings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeRating>>> GetRecipeRatings()
        {
            return await _context.RecipeRatings.ToListAsync();
        }

        // GET: api/RecipeRatings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeRating>> GetRecipeRating(int id)
        {
            var recipeRating = await _context.RecipeRatings.FindAsync(id);

            if (recipeRating == null)
            {
                return NotFound();
            }

            return recipeRating;
        }

        // PUT: api/RecipeRatings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipeRating(int id, RecipeRating recipeRating)
        {
            if (id != recipeRating.Id)
            {
                return BadRequest();
            }

            _context.Entry(recipeRating).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecipeRatingExists(id))
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

        // POST: api/RecipeRatings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RecipeRating>> PostRecipeRating(RecipeRating recipeRating)
        {
            _context.RecipeRatings.Add(recipeRating);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecipeRating", new { id = recipeRating.Id }, recipeRating);
        }

        // DELETE: api/RecipeRatings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipeRating(int id)
        {
            var recipeRating = await _context.RecipeRatings.FindAsync(id);
            if (recipeRating == null)
            {
                return NotFound();
            }

            _context.RecipeRatings.Remove(recipeRating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RecipeRatingExists(int id)
        {
            return _context.RecipeRatings.Any(e => e.Id == id);
        }
    }
}
