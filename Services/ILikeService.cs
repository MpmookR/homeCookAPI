using homeCookAPI.Models;

public interface ILikeService
{
    Task<IEnumerable<LikeDTO>> GetAllLikesAsync();
    Task<IEnumerable<LikeDTO>> GetLikesByRecipeIdAsync(int recipeId);
    Task<LikeDTO> AddLikeAsync(string userId, int recipeId);
    Task<bool> UnlikeRecipeAsync(string userId, int recipeId);
}
