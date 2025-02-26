using homeCookAPI.Models;

public class SavedRecipeService : ISavedRecipeService
{
    private readonly ISavedRecipeRepository _savedRecipeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRecipeRepository _recipeRepository;

    public SavedRecipeService(ISavedRecipeRepository savedRecipeRepository, IUserRepository userRepository, IRecipeRepository recipeRepository)
    {
        _savedRecipeRepository = savedRecipeRepository;
        _userRepository = userRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<IEnumerable<SavedRecipeDTO>> GetSavedRecipesByUserAsync(string userId)
    {
        var savedRecipes = await _savedRecipeRepository.GetSavedRecipesByUserAsync(userId);
        return savedRecipes.Select(MapToDTO);
    }

    public async Task<SavedRecipeDTO> SaveRecipeAsync(string userId, int recipeId)
    {
        if (!await _recipeRepository.ExistsAsync(recipeId))
            throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (await _savedRecipeRepository.ExistsAsync(userId, recipeId))
            throw new InvalidOperationException("Recipe is already saved by this user.");

        var savedRecipe = new SavedRecipe
        {
            UserId = userId,
            RecipeId = recipeId,
            CreatedAt = DateTime.UtcNow
        };

        await _savedRecipeRepository.AddAsync(savedRecipe);
        return MapToDTO(savedRecipe);
    }

    public async Task<bool> RemoveSavedRecipeAsync(int savedRecipeId, string userId)
    {
        var savedRecipe = await _savedRecipeRepository.GetByIdAsync(savedRecipeId);
        if (savedRecipe == null) throw new KeyNotFoundException($"Saved recipe with ID {savedRecipeId} not found.");

        if (savedRecipe.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to unsave this recipe.");

        await _savedRecipeRepository.DeleteAsync(savedRecipe);
        return true;
    }

    private static SavedRecipeDTO MapToDTO(SavedRecipe savedRecipe)
    {
        return new SavedRecipeDTO
        {
            SavedRecipeId = savedRecipe.SavedRecipeId,
            UserId = savedRecipe.UserId,
            RecipeId = savedRecipe.RecipeId,
            CreatedAt = savedRecipe.CreatedAt
        };
    }
}
