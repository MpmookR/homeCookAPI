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
    public class LikesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LikesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/likes
        // show all recipe that recieved like
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetLikes()
        {
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

            return Ok(likes);
        }

        // Get Likes for a Specific Recipe
        // GET: api/likes/recipe/1
        [HttpGet("recipe/{recipeId}")]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetLikesByRecipe(int recipeId)
        {
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
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

            return Ok(likes);
        }

        // POST: api/likes
        [HttpPost]
        public async Task<ActionResult<object>> PostLike(Like like)
        {
            // Ensure Recipe exists
            var recipe = await _context.Recipes.FindAsync(like.RecipeId);
            if (recipe == null)
            {
                return BadRequest(new { message = $"Recipe with ID {like.RecipeId} not found" });
            }

            // Ensure User exists
            var user = await _context.Users.FindAsync(like.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Prevent duplicate likes
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.RecipeId == like.RecipeId && l.UserId == like.UserId);
            if (existingLike != null)
            {
                return BadRequest(new { message = "User has already liked this recipe" });
            }

            // Save Like
            like.CreatedAt = DateTime.UtcNow;
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

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

        // Unlike a Recipe
        // DELETE: api/likes/1
        [HttpDelete("{likeId}")]
        public async Task<IActionResult> DeleteLike(int likeId)
        {
            var like = await _context.Likes.FindAsync(likeId);
            if (like == null)
            {
                return NotFound(new { message = "Like not found" });
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Like has been successfully removed!" });
        }
    }
}
