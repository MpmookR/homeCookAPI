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
    public class LikesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LikesController> _logger; 

        public LikesController(ApplicationDbContext context, ILogger<LikesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // api/likes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetLikes()
        {
            _logger.LogInformation("Fetching all likes from the database...");

            var likes = await _context.Likes
                .Include(l => l.User)
                .Include(l => l.Recipe)
                .Select(l => new LikeDTO
                {
                    LikeId = l.LikeId,
                    UserId = l.UserId,
                    UserName = l.User.FullName,
                    RecipeId = l.RecipeId,
                    RecipeName = l.Recipe.Name,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} likes.", likes.Count);
            return Ok(likes);
        }

        // api/likes/recipe/{recipeId}
        [HttpGet("recipe/{recipeId}")]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetLikesByRecipe(int recipeId)
        {
            _logger.LogInformation("Fetching likes for Recipe ID {RecipeId}.", recipeId);

            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Failed to fetch likes - Recipe ID {RecipeId} not found.", recipeId);
                return NotFound(new { message = "Recipe not found" });
            }

            var likes = await _context.Likes
                .Where(l => l.RecipeId == recipeId)
                .Include(l => l.User)
                .Select(l => new LikeDTO
                {
                    LikeId = l.LikeId,
                    UserId = l.UserId,
                    UserName = l.User.FullName,
                    RecipeId = l.RecipeId,
                    RecipeName = recipe.Name,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} likes for Recipe ID {RecipeId}.", likes.Count, recipeId);
            return Ok(likes);
        }

        // api/likes
        [HttpPost]
        public async Task<ActionResult<object>> PostLike(Like like)
        {
            _logger.LogInformation("User {UserId} is attempting to like Recipe ID {RecipeId}.", like.UserId, like.RecipeId);

            var recipe = await _context.Recipes.FindAsync(like.RecipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Failed to add like - Recipe ID {RecipeId} not found.", like.RecipeId);
                return BadRequest(new { message = $"Recipe with ID {like.RecipeId} not found" });
            }
           
            // Ensure User exists
            var user = await _context.Users.FindAsync(like.UserId);
            if (user == null)
            {
                _logger.LogWarning("Failed to add like - User ID {UserId} not found.", like.UserId);
                return BadRequest(new { message = "User not found" });
            }
            
            // Prevent duplicate likes
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.RecipeId == like.RecipeId && l.UserId == like.UserId);
            if (existingLike != null)
            {
                _logger.LogWarning("User {UserId} has already liked Recipe ID {RecipeId}.", like.UserId, like.RecipeId);
                return BadRequest(new { message = "User has already liked this recipe" });
            }
            
            // Save Like
            like.CreatedAt = DateTime.UtcNow;
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} successfully liked Recipe ID {RecipeId}.", like.UserId, like.RecipeId);

            return Ok(new
            {
                message = "Recipe successfully liked!",
                like = new LikeDTO
                {
                    LikeId = like.LikeId,
                    UserId = like.UserId,
                    UserName = user.FullName,
                    RecipeId = like.RecipeId,
                    RecipeName = recipe.Name,
                    CreatedAt = like.CreatedAt
                }
            });
        }

        // api/likes/{likeId}
        [HttpDelete("{likeId}")]
        public async Task<IActionResult> DeleteLike(int likeId)
        {
            _logger.LogInformation("Attempting to remove Like ID {LikeId}.", likeId);

            var like = await _context.Likes.FindAsync(likeId);
            if (like == null)
            {
                _logger.LogWarning("Failed to remove like - Like ID {LikeId} not found.", likeId);
                return NotFound(new { message = "Like not found" });
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Like ID {LikeId} removed successfully.", likeId);
            return Ok(new { message = "Like has been successfully removed!" });
        }
    }
}
