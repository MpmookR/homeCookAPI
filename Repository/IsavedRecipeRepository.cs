using homeCookAPI.Models;

public interface ISavedRecipeRepository
{
    Task<IEnumerable<SavedRecipe>> GetSavedRecipesByUserAsync(string userId);
    Task<bool> ExistsAsync(string userId, int recipeId);
    Task<SavedRecipe> GetByIdAsync(int savedRecipeId);
    Task AddAsync(SavedRecipe savedRecipe);
    Task DeleteAsync(SavedRecipe savedRecipe);
}
