using homeCookAPI.Models;

public interface IRecipeRatingService
{
    Task<IEnumerable<RecipeRatingDTO>> GetAllRatingsAsync();
    Task<IEnumerable<RecipeRatingDTO>> GetRatingsByRecipeIdAsync(int recipeId);
    Task<RecipeRatingDTO> GetRatingByIdAsync(int recipeRatingId);
    Task<RecipeRatingDTO> AddRatingAsync(RecipeRating rating);
    Task<RecipeRatingDTO> UpdateRatingAsync(int recipeRatingId, int newRating);
    Task<bool> RemoveRatingAsync(int recipeRatingId);
}
