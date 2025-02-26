using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

public class RecipeRatingRepository : IRecipeRatingRepository
{
    private readonly ApplicationDbContext _context;

    public RecipeRatingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RecipeRating>> GetAllAsync()
    {
        return await _context.RecipeRatings.Include(rt => rt.User).ToListAsync();
    }

    public async Task<IEnumerable<RecipeRating>> GetByRecipeIdAsync(int recipeId)
    {
        return await _context.RecipeRatings
            .Where(rt => rt.RecipeId == recipeId)
            .Include(rt => rt.User)
            .ToListAsync();
    }

    public async Task<RecipeRating> GetByIdAsync(int recipeRatingId)
    {
        return await _context.RecipeRatings.FindAsync(recipeRatingId);
    }

    public async Task<RecipeRating> GetByUserAndRecipeAsync(string userId, int recipeId)
    {
        return await _context.RecipeRatings.FirstOrDefaultAsync(rt => rt.UserId == userId && rt.RecipeId == recipeId);
    }

    public async Task<bool> ExistsAsync(int recipeId, string userId)
    {
        return await _context.RecipeRatings.AnyAsync(rt => rt.RecipeId == recipeId && rt.UserId == userId);
    }

    public async Task AddAsync(RecipeRating rating)
    {
        await _context.RecipeRatings.AddAsync(rating);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RecipeRating rating)
    {
        _context.RecipeRatings.Update(rating);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RecipeRating rating)
    {
        _context.RecipeRatings.Remove(rating);
        await _context.SaveChangesAsync();
    }
}
