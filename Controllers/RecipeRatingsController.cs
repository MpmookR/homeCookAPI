using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using Microsoft.Extensions.Logging;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeRatingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecipeRatingsController> _logger; 

        public RecipeRatingsController(ApplicationDbContext context, ILogger<RecipeRatingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/RecipeRatings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeRatingDTO>>> GetRecipeRatings()
        {
            _logger.LogInformation("Fetching all recipe ratings...");

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

            _logger.LogInformation("Successfully retrieved {Count} ratings.", ratings.Count);
            return Ok(ratings);
        }

        // GET: api/RecipeRatings/recipe/{recipeId}
        [HttpGet("recipe/{recipeId}")]
        public async Task<ActionResult<IEnumerable<RecipeRatingDTO>>> GetRatingsForRecipe(int recipeId)
        {
            _logger.LogInformation("Fetching ratings for Recipe ID {RecipeId}.", recipeId);

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
                _logger.LogWarning("No ratings found for Recipe ID {RecipeId}.", recipeId);
                return NotFound(new { message = "No ratings found for this recipe." });
            }

            _logger.LogInformation("Successfully retrieved {Count} ratings for Recipe ID {RecipeId}.", ratings.Count, recipeId);
            return Ok(ratings);
        }

        // GET: api/RecipeRatings/{recipeRatingId}
        [HttpGet("{recipeRatingId}")]
        public async Task<ActionResult<RecipeRatingDTO>> GetRecipeRating(int recipeRatingId)
        {
            _logger.LogInformation("Fetching rating with ID {RecipeRatingId}.", recipeRatingId);

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
                _logger.LogWarning("Rating ID {RecipeRatingId} not found.", recipeRatingId);
                return NotFound(new { message = "Rating not found." });
            }

            _logger.LogInformation("Successfully retrieved rating ID {RecipeRatingId}.", recipeRatingId);
            return Ok(rating);
        }

        // POST: api/RecipeRatings
        [HttpPost]
        public async Task<ActionResult<RecipeRatingDTO>> PostRecipeRating(RecipeRating rating)
        {
            _logger.LogInformation("User {UserId} is attempting to rate Recipe ID {RecipeId}.", rating.UserId, rating.RecipeId);

            var recipe = await _context.Recipes.FindAsync(rating.RecipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Failed to add rating - Recipe ID {RecipeId} not found.", rating.RecipeId);
                return BadRequest(new { message = "Recipe not found." });
            }

            // Ensure User Exists
            var user = await _context.Users.FindAsync(rating.UserId);
            if (user == null)
            {
                _logger.LogWarning("Failed to add rating - User ID {UserId} not found.", rating.UserId);
                return BadRequest(new { message = "User not found." });
            }
            
            // Prevent duplicate ratings
            var existingRating = await _context.RecipeRatings
                .FirstOrDefaultAsync(rt => rt.RecipeId == rating.RecipeId && rt.UserId == rating.UserId);
            if (existingRating != null)
            {
                _logger.LogWarning("User {UserId} has already rated Recipe ID {RecipeId}.", rating.UserId, rating.RecipeId);
                return BadRequest(new { message = "You have already rated this recipe." });
            }
            
            // save Rating
            _context.RecipeRatings.Add(rating);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} successfully rated Recipe ID {RecipeId}.", rating.UserId, rating.RecipeId);

            return CreatedAtAction(nameof(GetRecipeRating), new { recipeRatingId = rating.RecipeRatingId }, new RecipeRatingDTO
            {
                RecipeRatingId = rating.RecipeRatingId,
                UserId = rating.UserId,
                UserName = user.FullName,
                RecipeId = rating.RecipeId,
                Rating = rating.Rating
            });
        }

        // PUT: api/RecipeRatings/{recipeRatingId}
        [HttpPut("{recipeRatingId}")]
        public async Task<IActionResult> PutRecipeRating(int recipeRatingId, RecipeRating ratingUpdate)
        {
            _logger.LogInformation("Attempting to update rating ID {RecipeRatingId}.", recipeRatingId);

            var existingRating = await _context.RecipeRatings.FindAsync(recipeRatingId);
            if (existingRating == null)
            {
                _logger.LogWarning("Failed to update - Rating ID {RecipeRatingId} not found.", recipeRatingId);
                return NotFound(new { message = "Rating not found." });
            }

            existingRating.Rating = ratingUpdate.Rating;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated rating ID {RecipeRatingId}.", recipeRatingId);
            return Ok(new { message = "Rating updated successfully!", updatedRating = existingRating.Rating });
        }

        // DELETE: api/RecipeRatings/{recipeRatingId}
        [HttpDelete("{recipeRatingId}")]
        public async Task<IActionResult> DeleteRecipeRating(int recipeRatingId)
        {
            _logger.LogInformation("Attempting to delete rating ID {RecipeRatingId}.", recipeRatingId);

            var rating = await _context.RecipeRatings.FindAsync(recipeRatingId);
            if (rating == null)
            {
                _logger.LogWarning("Failed to delete - Rating ID {RecipeRatingId} not found.", recipeRatingId);
                return NotFound(new { message = "Rating not found." });
            }

            _context.RecipeRatings.Remove(rating);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted rating ID {RecipeRatingId}.", recipeRatingId);
            return Ok(new { message = "Rating successfully deleted!" });
        }
    }
}
