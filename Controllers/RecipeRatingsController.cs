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

        // GET: All Ratings
        // /api/RecipeRatings	
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeRatingDTO>>> GetRecipeRatings()
        {
            var ratings = await _context.RecipeRatings
                .Include(rt => rt.User)
                .Select(rt => new RecipeRatingDTO
                {
                    RecipeRatingId = rt.RecipeRatingId,
                    UserId = rt.UserId,
                    UserName = rt.User.FullName,
                    RecipeId = rt.RecipeId,
                    Rating = rt.Rating,
                })
                .ToListAsync();

            return Ok(ratings);
        }

        // GET ratings for a Specific Recipe
        // api/RecipeRatings/recipe/{recipeId}	
        [HttpGet("recipe/{recipeId}")]
        public async Task<ActionResult<IEnumerable<RecipeRatingDTO>>> GetRatingsForRecipe(int recipeId)
        {
            var ratings = await _context.RecipeRatings
                .Include(rt => rt.User)
                .Where(rt => rt.RecipeId == recipeId)
                .Select(rt => new RecipeRatingDTO
                {
                    RecipeRatingId = rt.RecipeRatingId,
                    UserId = rt.UserId,
                    UserName = rt.User.FullName,
                    RecipeId = rt.RecipeId,
                    Rating = rt.Rating,
                })
                .ToListAsync();

            if (!ratings.Any())
            {
                return NotFound(new { message = "No ratings found for this recipe." });
            }

            return Ok(ratings);
        }

        // Get a Single Rating by ID : what recipe get 5 stars
        // /api/RecipeRatings/{recipeRatingId}	
        [HttpGet("{recipeRatingId}")]
        public async Task<ActionResult<RecipeRatingDTO>> GetRecipeRating(int recipeRatingId)
        {
            var rating = await _context.RecipeRatings
                .Include(rt => rt.User)
                .Where(rt => rt.RecipeRatingId == recipeRatingId)
                .Select(rt => new RecipeRatingDTO
                {
                    RecipeRatingId = rt.RecipeRatingId,
                    UserId = rt.UserId,
                    UserName = rt.User.FullName,
                    RecipeId = rt.RecipeId,
                    Rating = rt.Rating,
                })
                .FirstOrDefaultAsync();

            if (rating == null)
            {
                return NotFound(new { message = "Rating not found." });
            }

            return Ok(rating);
        }

        // Add a Rating (Prevents Duplicate Ratings)
        // api/RecipeRatings	
        [HttpPost]
        public async Task<ActionResult<RecipeRatingDTO>> PostRecipeRating(RecipeRating rating)
        {
            // Ensure Recipe Exists
            var recipe = await _context.Recipes.FindAsync(rating.RecipeId);
            if (recipe == null)
            {
                return BadRequest(new { message = "Recipe not found." });
            }

            // Ensure User Exists
            var user = await _context.Users.FindAsync(rating.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            // Prevent duplicate ratings
            var existingRating = await _context.RecipeRatings
                .FirstOrDefaultAsync(rt => rt.RecipeId == rating.RecipeId && rt.UserId == rating.UserId);
            if (existingRating != null)
            {
                return BadRequest(new { message = "You have already rated this recipe." });
            }

            // save Rating
            _context.RecipeRatings.Add(rating);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecipeRating), new { recipeRatingId = rating.RecipeRatingId }, new RecipeRatingDTO
            {
                RecipeRatingId = rating.RecipeRatingId,
                UserId = rating.UserId,
                UserName = user.FullName,
                RecipeId = rating.RecipeId,
                Rating = rating.Rating
            });
        }

        // Update a Rating (Only the user who rated can edit)
        // api/RecipeRatings/{recipeRatingId}
        [HttpPut("{recipeRatingId}")]
        public async Task<IActionResult> PutRecipeRating(int recipeRatingId, RecipeRating ratingUpdate)
        {
            var existingRating = await _context.RecipeRatings.FindAsync(recipeRatingId);
            if (existingRating == null)
            {
                return NotFound(new { message = "Rating not found." });
            }

            existingRating.Rating = ratingUpdate.Rating;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RecipeRatings.Any(rt => rt.RecipeRatingId == recipeRatingId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Rating updated successfully!", updatedRating = existingRating.Rating });
        }

        // Remove a Rating (Only the user who rated can delete)
        // api/RecipeRatings/{recipeRatingId}
        [HttpDelete("{recipeRatingId}")]
        public async Task<IActionResult> DeleteRecipeRating(int recipeRatingId)
        {
            var rating = await _context.RecipeRatings.FindAsync(recipeRatingId);
            if (rating == null)
            {
                return NotFound(new { message = "Rating not found." });
            }

            _context.RecipeRatings.Remove(rating);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rating successfully deleted!" });
        }
    }
}
