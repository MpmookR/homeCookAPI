using homeCookAPI.Models;

public interface ISavedRecipeService
{
    Task<IEnumerable<SavedRecipeDTO>> GetSavedRecipesByUserAsync(string userId);
    Task<SavedRecipeDTO> SaveRecipeAsync(string userId, int recipeId);
    Task<bool> RemoveSavedRecipeAsync(int savedRecipeId, string userId);
}
