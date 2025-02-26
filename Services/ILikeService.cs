using homeCookAPI.Models;

public interface ILikeService
{
    Task<IEnumerable<LikeDTO>> GetAllLikesAsync();
    Task<IEnumerable<LikeDTO>> GetLikesByRecipeIdAsync(int recipeId);
    Task<LikeDTO> AddLikeAsync(Like like);
    Task<bool> UnlikeRecipeAsync(string userId, int recipeId);
}
