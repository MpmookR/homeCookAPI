using homeCookAPI.Models;

public interface ILikeRepository
{
    Task<IEnumerable<Like>> GetAllAsync();
    Task<IEnumerable<Like>> GetByRecipeIdAsync(int recipeId);
    Task<Like> GetByUserAndRecipeAsync(string userId, int recipeId); //unlike using userId and recipeId
    Task<bool> ExistsAsync(int recipeId, string userId);
    Task AddAsync(Like like);
    Task DeleteAsync(Like like);
}

