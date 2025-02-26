using System.Collections.Generic;
using System.Threading.Tasks;
using homeCookAPI.Models;

public interface IRecipeService
{
    Task<IEnumerable<RecipeDTO>> GetAllRecipesAsync();
    Task<RecipeDTO> GetRecipeByIdAsync(int id);
    Task<RecipeDTO> AddRecipeAsync(RecipeDTO recipeDTO);
    Task<RecipeDTO> UpdateRecipeAsync(int id, RecipeDTO recipeDTO, string userId);
    Task<bool> DeleteRecipeAsync(int id, string userId, bool isAdmin);
}
