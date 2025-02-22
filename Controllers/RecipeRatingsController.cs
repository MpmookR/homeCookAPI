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
        [HttpGet("{recipeRatingId}")]
        public async Task<ActionResult<RecipeRating>> GetRecipeRating(int recipeRatingId)
        {
            var recipeRating = await _context.RecipeRatings.FindAsync(recipeRatingId);
            if (recipeRating == null)
            {
                return NotFound();
            }

            return recipeRating;
        }

        // PUT: api/RecipeRatings/5
        [HttpPut("{recipeRatingId}")]
        public async Task<IActionResult> PutRecipeRating(int recipeRatingId, RecipeRating recipeRating)
        {
            if (recipeRatingId != recipeRating.RecipeRatingId) // ✅ Updated from Id to RecipeRatingId
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
                if (!RecipeRatingExists(recipeRatingId))
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
        [HttpPost]
        public async Task<ActionResult<RecipeRating>> PostRecipeRating(RecipeRating recipeRating)
        {
            _context.RecipeRatings.Add(recipeRating);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecipeRating", new { recipeRatingId = recipeRating.RecipeRatingId }, recipeRating);
        }

        // DELETE: api/RecipeRatings/5
        [HttpDelete("{recipeRatingId}")]
        public async Task<IActionResult> DeleteRecipeRating(int recipeRatingId)
        {
            var recipeRating = await _context.RecipeRatings.FindAsync(recipeRatingId);
            if (recipeRating == null)
            {
                return NotFound();
            }

            _context.RecipeRatings.Remove(recipeRating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RecipeRatingExists(int recipeRatingId)
        {
            return _context.RecipeRatings.Any(e => e.RecipeRatingId == recipeRatingId);
        }
    }
}
