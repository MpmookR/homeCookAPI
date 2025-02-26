using homeCookAPI.Models;

public interface IRecipeRatingRepository
{
    Task<IEnumerable<RecipeRating>> GetAllAsync();
    Task<IEnumerable<RecipeRating>> GetByRecipeIdAsync(int recipeId);
    Task<RecipeRating> GetByIdAsync(int recipeRatingId);
    Task<RecipeRating> GetByUserAndRecipeAsync(string userId, int recipeId);
    Task<bool> ExistsAsync(int recipeId, string userId);
    Task AddAsync(RecipeRating rating);
    Task UpdateAsync(RecipeRating rating);
    Task DeleteAsync(RecipeRating rating);
}
