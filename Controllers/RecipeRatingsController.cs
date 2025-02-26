using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

namespace homeCookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeRatingsController : ControllerBase
    {
        private readonly IRecipeRatingService _recipeRatingService;
        private readonly ILogger<RecipeRatingsController> _logger;

        public RecipeRatingsController(IRecipeRatingService recipeRatingService, ILogger<RecipeRatingsController> logger)
        {
            _recipeRatingService = recipeRatingService;
            _logger = logger;
        }

        // GET: api/RecipeRatings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeRatingDTO>>> GetRecipeRatings()
        {
            _logger.LogInformation("Fetching all recipe ratings...");
            var ratings = await _recipeRatingService.GetAllRatingsAsync();
            _logger.LogInformation("Successfully retrieved {Count} ratings.", ratings.Count());
            return Ok(ratings);
        }


        // GET: api/RecipeRatings/recipe/{recipeId}
        [HttpGet("recipe/{recipeId}")]
        public async Task<ActionResult<IEnumerable<RecipeRatingDTO>>> GetRatingsForRecipe(int recipeId)
        {
            _logger.LogInformation("Fetching ratings for Recipe ID {RecipeId}.", recipeId);
            try
            {
                var ratings = await _recipeRatingService.GetRatingsByRecipeIdAsync(recipeId);
                _logger.LogInformation("Successfully retrieved {Count} ratings for Recipe ID {RecipeId}.", ratings.Count(), recipeId);
                return Ok(ratings);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/RecipeRatings/{recipeRatingId}
        [HttpGet("{recipeRatingId}")]
        public async Task<ActionResult<RecipeRatingDTO>> GetRecipeRating(int recipeRatingId)
        {
            _logger.LogInformation("Fetching rating with ID {RecipeRatingId}.", recipeRatingId);
            try
            {
                var rating = await _recipeRatingService.GetRatingByIdAsync(recipeRatingId);
                _logger.LogInformation("Successfully retrieved rating ID {RecipeRatingId}.", recipeRatingId);
                return Ok(rating);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }


        // POST: api/RecipeRatings
        [HttpPost]
        public async Task<ActionResult<RecipeRatingDTO>> PostRecipeRating(RecipeRating rating)
        {
            _logger.LogInformation("User {UserId} is rating Recipe ID {RecipeId}.", rating.UserId, rating.RecipeId);
            try
            {
                var newRating = await _recipeRatingService.AddRatingAsync(rating);
                _logger.LogInformation("User {UserId} successfully rated Recipe ID {RecipeId}.", rating.UserId, rating.RecipeId);
                return CreatedAtAction(nameof(GetRecipeRating), new { recipeRatingId = newRating.RecipeRatingId }, newRating);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/RecipeRatings/{recipeRatingId}
        [HttpPut("{recipeRatingId}")]
        public async Task<IActionResult> PutRecipeRating(int recipeRatingId, [FromBody] int newRating)
        {
            _logger.LogInformation("Attempting to update rating ID {RecipeRatingId}.", recipeRatingId);
            try
            {
                var updatedRating = await _recipeRatingService.UpdateRatingAsync(recipeRatingId, newRating);
                _logger.LogInformation("Successfully updated rating ID {RecipeRatingId}.", recipeRatingId);
                return Ok(new { message = "Rating updated successfully!", updatedRating = updatedRating.Rating });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/RecipeRatings/{recipeRatingId}
        [HttpDelete("{recipeRatingId}")]
        public async Task<IActionResult> DeleteRecipeRating(int recipeRatingId)
        {
            _logger.LogInformation("Attempting to delete rating ID {RecipeRatingId}.", recipeRatingId);
            try
            {
                await _recipeRatingService.RemoveRatingAsync(recipeRatingId);
                _logger.LogInformation("Successfully deleted rating ID {RecipeRatingId}.", recipeRatingId);
                return Ok(new { message = "Rating successfully deleted!" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
