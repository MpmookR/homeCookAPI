using System.Collections.Generic;
using System.Threading.Tasks;
using homeCookAPI.Models;

public interface IRecipeRepository
{
    Task<IEnumerable<Recipe>> GetAllAsync();
    Task<Recipe> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task AddAsync(Recipe recipe);
    Task UpdateAsync(Recipe recipe);
    Task DeleteAsync(Recipe recipe);
}
