using homeCookAPI.Models;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRecipeRepository _recipeRepository;

    public LikeService(ILikeRepository likeRepository, IUserRepository userRepository, IRecipeRepository recipeRepository)
    {
        _likeRepository = likeRepository;
        _userRepository = userRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<IEnumerable<LikeDTO>> GetAllLikesAsync()
    {
        var likes = await _likeRepository.GetAllAsync();
        return likes.Select(MapToDTO);
    }

    public async Task<IEnumerable<LikeDTO>> GetLikesByRecipeIdAsync(int recipeId)
    {
        if (!await _recipeRepository.ExistsAsync(recipeId))
            throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        var likes = await _likeRepository.GetByRecipeIdAsync(recipeId);
        return likes.Select(MapToDTO);
    }

    public async Task<LikeDTO> AddLikeAsync(string userId, int recipeId)
    {
        if (!await _recipeRepository.ExistsAsync(recipeId))
            throw new KeyNotFoundException($"Recipe with ID {recipeId} not found.");

        if (!await _userRepository.ExistsAsync(userId))
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (await _likeRepository.ExistsAsync(recipeId, userId))
            throw new InvalidOperationException("User has already liked this recipe.");

        var like = new Like
        {
            UserId = userId,
            RecipeId = recipeId,
            CreatedAt = DateTime.UtcNow
        };

        await _likeRepository.AddAsync(like);

        // Fetch the full Like object including Recipe after saving
        var savedLike = await _likeRepository.GetByUserAndRecipeAsync(userId, recipeId);

        return MapToDTO(savedLike);
    }


    public async Task<bool> UnlikeRecipeAsync(string userId, int recipeId)
    {
        var like = await _likeRepository.GetByUserAndRecipeAsync(userId, recipeId);
        if (like == null) throw new KeyNotFoundException("User has not liked this recipe.");

        await _likeRepository.DeleteAsync(like);
        return true;
    }

    private static LikeDTO MapToDTO(Like like)
    {
        return new LikeDTO
        {
            LikeId = like.LikeId,
            UserId = like.UserId,
            UserName = like.User?.FullName,
            RecipeId = like.RecipeId,
            CreatedAt = like.CreatedAt
        };
    }
}
