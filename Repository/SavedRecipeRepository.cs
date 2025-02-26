using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

public class SavedRecipeRepository : ISavedRecipeRepository
{
    private readonly ApplicationDbContext _context;

    public SavedRecipeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SavedRecipe>> GetSavedRecipesByUserAsync(string userId)
    {
        return await _context.SavedRecipes
            .Where(s => s.UserId == userId)
            .Include(s => s.Recipe)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string userId, int recipeId)
    {
        return await _context.SavedRecipes.AnyAsync(s => s.UserId == userId && s.RecipeId == recipeId);
    }

    public async Task<SavedRecipe> GetByIdAsync(int savedRecipeId)
    {
        return await _context.SavedRecipes.FindAsync(savedRecipeId);
    }

    public async Task AddAsync(SavedRecipe savedRecipe)
    {
        await _context.SavedRecipes.AddAsync(savedRecipe);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SavedRecipe savedRecipe)
    {
        _context.SavedRecipes.Remove(savedRecipe);
        await _context.SaveChangesAsync();
    }
}
