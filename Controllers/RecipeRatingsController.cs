using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        /// <summary>
        /// Retrieves all recipe ratings
        /// </summary>
        /// <returns>A list of all recipe ratings</returns>
        /// <response code="200">Returns the list of recipe ratings</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeRatingDTO>>> GetRecipeRatings()
        {
            _logger.LogInformation("Fetching all recipe ratings...");
            var ratings = await _recipeRatingService.GetAllRatingsAsync();
            _logger.LogInformation("Successfully retrieved {Count} ratings.", ratings.Count());
            return Ok(ratings);
        }


        /// <summary>
        /// Retrieves ratings for a specific recipe.
        /// </summary>
        /// <param name="recipeId">The unique identifier of the recipe.</param>
        /// <returns>A list of ratings for the specified recipe.</returns>
        /// <response code="200">Returns the list of ratings</response>
        /// <response code="404">Recipe not found</response>
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

        /// <summary>
        /// Adds a new rating for a recipe
        /// </summary>
        /// <param name="request">The recipe rating details</param>
        /// <returns>The newly created rating</returns>
        /// <response code="201">Recipe rating added successfully</response>
        /// <response code="400">Invalid request</response>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RecipeRatingDTO>> PostRecipeRating([FromBody] RecipeRatingDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized request: Unable to retrieve UserId from authentication.");
                return Unauthorized(new { message = "Unauthorized request: Unable to determine user." });
            }

            _logger.LogInformation("User {UserId} is rating Recipe ID {RecipeId}.", userId, request.RecipeId);

            try
            {
                var newRating = await _recipeRatingService.AddRatingAsync(userId, request.RecipeId, request.Rating);
                _logger.LogInformation("User {UserId} successfully rated Recipe ID {RecipeId}.", userId, request.RecipeId);
                return CreatedAtAction(nameof(GetRatingsForRecipe), new { recipeId = newRating.RecipeId }, newRating);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Updates an existing rating
        /// </summary>
        /// <param name="recipeRatingId">The rating ID</param>
        /// <param name="request">Updated rating details</param>
        /// <returns>The updated rating</returns>
        /// <response code="200">Rating updated successfully</response>
        /// <response code="403">Unauthorized access</response>
        [Authorize]
        [HttpPut("{recipeRatingId}")]
        public async Task<IActionResult> PutRecipeRating(int recipeRatingId, [FromBody] RecipeRatingDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                _logger.LogWarning("Unauthorized request: Unable to retrieve UserId from authentication.");
                return Unauthorized(new { message = "Unauthorized request: Unable to determine user." });
            }

            _logger.LogInformation("User {UserId} is attempting to update rating ID {RecipeRatingId}.", userId, recipeRatingId);

            try
            {
                var updatedRating = await _recipeRatingService.UpdateRatingAsync(recipeRatingId, userId, request.RecipeId, request.Rating);
                _logger.LogInformation("Successfully updated rating ID {RecipeRatingId}.", recipeRatingId);
                return Ok(new { message = "Rating updated successfully!", updatedRating = updatedRating.Rating });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
        }


         /// <summary>
        /// Deletes a rating.
        /// </summary>
        /// <param name="recipeRatingId">The ID of the rating to delete.</param>
        /// <returns>A confirmation message if successful.</returns>
        /// <response code="200">Rating deleted successfully</response>
        /// <response code="403">Unauthorized access</response>
        [Authorize]
        [HttpDelete("{recipeRatingId}")]
        public async Task<IActionResult> DeleteRecipeRating(int recipeRatingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

            _logger.LogInformation("User {UserId} is attempting to delete rating ID {RecipeRatingId}.", userId, recipeRatingId);

            try
            {
                await _recipeRatingService.RemoveRatingAsync(recipeRatingId, userId);
                _logger.LogInformation("Successfully deleted rating ID {RecipeRatingId}.", recipeRatingId);
                return Ok(new { message = "Rating successfully deleted!" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
        }

    }
}
