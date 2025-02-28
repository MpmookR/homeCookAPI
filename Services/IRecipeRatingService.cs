using homeCookAPI.Models;

public interface IRecipeRatingService
{
    Task<IEnumerable<RecipeRatingDTO>> GetAllRatingsAsync();
    Task<IEnumerable<RecipeRatingDTO>> GetRatingsByRecipeIdAsync(int recipeId);
    Task<RecipeRatingDTO> GetRatingByIdAsync(int recipeRatingId);
    Task<RecipeRatingDTO> AddRatingAsync(string userId, int recipeId, int ratingValue);
    Task<RecipeRatingDTO> UpdateRatingAsync(int recipeRatingId, string userId, int recipeId, int newRating);
    Task<bool> RemoveRatingAsync(int recipeRatingId, string userId);
}
