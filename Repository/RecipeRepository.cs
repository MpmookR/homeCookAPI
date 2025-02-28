using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

public class RecipeRepository : IRecipeRepository
{
    private readonly ApplicationDbContext _context;

    public RecipeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Recipe>> GetAllAsync()
    {
        return await _context.Recipes
            .Include(r => r.User)
            .Include(r => r.Comments)
            .Include(r => r.Ratings)
            .Include(r => r.Likes)
            .Include(r => r.SavedRecipes)
            .ToListAsync();
    }

public async Task<Recipe?> GetByIdAsync(int id)
{
    //Ensures all related data is available for MapToDTO()
    return await _context.Recipes
        .Where(r => r.RecipeId == id)
        .Include(r => r.Comments) 
        .Include(r => r.Likes) 
        .Include(r => r.SavedRecipes) 
        .Include(r => r.Ratings)
        .FirstOrDefaultAsync();
}

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Recipes.AnyAsync(r => r.RecipeId == id);
    }

    public async Task AddAsync(Recipe recipe)
    {
        await _context.Recipes.AddAsync(recipe);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Recipe recipe)
    {
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Recipe recipe)
    {
        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();
    }
}
