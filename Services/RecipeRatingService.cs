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

    public async Task<RecipeRatingDTO> AddRatingAsync(string userId, int recipeId, int ratingValue)
    {
        if (!await _recipeRepository.ExistsAsync(recipeId))
            throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (!await _userRepository.ExistsAsync(userId))
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (await _recipeRatingRepository.ExistsAsync(recipeId, userId))
            throw new InvalidOperationException("User has already rated this recipe.");

        var rating = new RecipeRating
        {
            UserId = userId,
            RecipeId = recipeId,
            Rating = ratingValue,
        };

        await _recipeRatingRepository.AddAsync(rating);

        // Fetch the full rating including Recipe after saving
        var savedRating = await _recipeRatingRepository.GetByUserAndRecipeAsync(userId, recipeId);

        return MapToDTO(savedRating);
    }


    public async Task<RecipeRatingDTO> UpdateRatingAsync(int recipeRatingId, string userId, int recipeId, int newRating)
{
    var rating = await _recipeRatingRepository.GetByIdAsync(recipeRatingId);
    if (rating == null)
        throw new KeyNotFoundException($"Rating with ID {recipeRatingId} not found.");

    if (rating.UserId != userId)
        throw new UnauthorizedAccessException("You are not authorized to update this rating.");

    rating.Rating = newRating;
    await _recipeRatingRepository.UpdateAsync(rating);

    return MapToDTO(rating);
}

    public async Task<bool> RemoveRatingAsync(int recipeRatingId, string userId)
{
    var rating = await _recipeRatingRepository.GetByIdAsync(recipeRatingId);
    if (rating == null)
        throw new KeyNotFoundException($"Rating with ID {recipeRatingId} not found.");

    if (rating.UserId != userId)
        throw new UnauthorizedAccessException("You are not authorized to delete this rating.");

    await _recipeRatingRepository.DeleteAsync(rating);
    return true;
}


    private static RecipeRatingDTO MapToDTO(RecipeRating rating)
    {
        return new RecipeRatingDTO
        {
            RecipeRatingId = rating.RecipeRatingId,
            // UserId = rating.UserId,
            // UserName = rating.User?.FullName,
            RecipeId = rating.RecipeId,
            Rating = rating.Rating
        };
    }
}
