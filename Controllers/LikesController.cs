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

        // api/likes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLikes()
        {
            var likes = await _context.Likes
                .Include(l => l.User)
                .Include(l => l.Recipe)
                .Select(l => new
                {
                    l.LikeId,
                    l.CreatedAt,
                    User = l.User != null ? new { l.User.Id, l.User.FullName } : null,
                    Recipe = l.Recipe != null ? new { l.RecipeId, l.Recipe.Name } : null
                })
                .ToListAsync();

            return Ok(likes);
        }

        // api/likes/1
        [HttpGet("{likeId}")]
        public async Task<ActionResult<object>> GetLike(int likeId)
        {
            var like = await _context.Likes
                .Include(l => l.User)
                .Include(l => l.Recipe)
                .Where(l => l.LikeId == likeId)
                .Select(l => new
                {
                    l.LikeId,
                    l.CreatedAt,
                    User = l.User != null ? new { l.User.Id, l.User.FullName } : null,
                    Recipe = l.Recipe != null ? new { l.RecipeId, l.Recipe.Name } : null
                })
                .FirstOrDefaultAsync();

            if (like == null)
            {
                return NotFound(new { message = "Like not found" });
            }

            return Ok(like);
        }

        // POST: Like a recipe
        // api/likes
        [HttpPost]
        public async Task<ActionResult<object>> PostLike(Like like)
        {
            // Ensure Recipe exists
            // var recipe = await _context.Recipes.FindAsync(like.RecipeId);
            // if (recipe == null)
            // {
            //     return BadRequest(new { message = "Recipe not found" });
            // }

            var recipe = await _context.Recipes
        .Where(r => r.RecipeId == like.RecipeId)
        .FirstOrDefaultAsync();

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

            // Prevent duplicate likes by the same user for the same recipe
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.RecipeId == like.RecipeId && l.UserId == like.UserId);
            if (existingLike != null)
            {
                return BadRequest(new { message = "User has already liked this recipe" });
            }

            like.CreatedAt = DateTime.UtcNow;
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Recipe successfully liked!",
                like = new
                {
                    like.LikeId,
                    like.CreatedAt,
                    like.UserId,
                    like.RecipeId
                }
            });
        }

        // DELETE: Unlike a recipe
        //api/likes/1
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
