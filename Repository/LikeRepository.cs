using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

public class LikeRepository : ILikeRepository
{
    private readonly ApplicationDbContext _context;

    public LikeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Like>> GetAllAsync()
    {
        return await _context.Likes.Include(l => l.User).Include(l => l.Recipe).ToListAsync();
    }

    public async Task<IEnumerable<Like>> GetByRecipeIdAsync(int recipeId)
    {
        return await _context.Likes
            .Where(l => l.RecipeId == recipeId)
            .Include(l => l.User)
            .ToListAsync();
    }

    public async Task<Like> GetByUserAndRecipeAsync(string userId, int recipeId)
    {
        return await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.RecipeId == recipeId);
    }

    public async Task<bool> ExistsAsync(int recipeId, string userId)
    {
        return await _context.Likes.AnyAsync(l => l.RecipeId == recipeId && l.UserId == userId);
    }

    public async Task AddAsync(Like like)
    {
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Like like)
    {
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
    }
}
