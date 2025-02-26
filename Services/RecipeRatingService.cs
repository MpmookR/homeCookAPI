using homeCookAPI.Models;

public class RecipeRatingService : IRecipeRatingService
{
    private readonly IRecipeRatingRepository _recipeRatingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRecipeRepository _recipeRepository;

    public RecipeRatingService(IRecipeRatingRepository recipeRatingRepository, IUserRepository userRepository, IRecipeRepository recipeRepository)
    {
        _recipeRatingRepository = recipeRatingRepository;
        _userRepository = userRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<IEnumerable<RecipeRatingDTO>> GetAllRatingsAsync()
    {
        var ratings = await _recipeRatingRepository.GetAllAsync();
        return ratings.Select(MapToDTO);
    }

    public async Task<IEnumerable<RecipeRatingDTO>> GetRatingsByRecipeIdAsync(int recipeId)
    {
        if (!await _recipeRepository.ExistsAsync(recipeId))
            throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        var ratings = await _recipeRatingRepository.GetByRecipeIdAsync(recipeId);
        return ratings.Select(MapToDTO);
    }

    public async Task<RecipeRatingDTO> GetRatingByIdAsync(int recipeRatingId)
    {
        var rating = await _recipeRatingRepository.GetByIdAsync(recipeRatingId);
        if (rating == null) throw new KeyNotFoundException($"Rating with ID {recipeRatingId} not found.");
        return MapToDTO(rating);
    }

    public async Task<RecipeRatingDTO> AddRatingAsync(RecipeRating rating)
    {
        if (!await _recipeRepository.ExistsAsync(rating.RecipeId))
            throw new KeyNotFoundException($"Recipe with ID {rating.RecipeId} not found.");

        if (!await _userRepository.ExistsAsync(rating.UserId))
            throw new KeyNotFoundException($"User with ID {rating.UserId} not found.");

        if (await _recipeRatingRepository.ExistsAsync(rating.RecipeId, rating.UserId))
            throw new InvalidOperationException("User has already rated this recipe.");

        await _recipeRatingRepository.AddAsync(rating);
        return MapToDTO(rating);
    }

    public async Task<RecipeRatingDTO> UpdateRatingAsync(int recipeRatingId, int newRating)
    {
        var rating = await _recipeRatingRepository.GetByIdAsync(recipeRatingId);
        if (rating == null) throw new KeyNotFoundException($"Rating with ID {recipeRatingId} not found.");

        rating.Rating = newRating;
        await _recipeRatingRepository.UpdateAsync(rating);
        return MapToDTO(rating);
    }

    public async Task<bool> RemoveRatingAsync(int recipeRatingId)
    {
        var rating = await _recipeRatingRepository.GetByIdAsync(recipeRatingId);
        if (rating == null) throw new KeyNotFoundException($"Rating with ID {recipeRatingId} not found.");

        await _recipeRatingRepository.DeleteAsync(rating);
        return true;
    }

    private static RecipeRatingDTO MapToDTO(RecipeRating rating)
    {
        return new RecipeRatingDTO
        {
            RecipeRatingId = rating.RecipeRatingId,
            UserId = rating.UserId,
            UserName = rating.User?.FullName,
            RecipeId = rating.RecipeId,
            Rating = rating.Rating
        };
    }
}
