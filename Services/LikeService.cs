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

    public async Task<LikeDTO> AddLikeAsync(Like like)
    {
        if (!await _recipeRepository.ExistsAsync(like.RecipeId))
            throw new KeyNotFoundException($"Recipe with ID {like.RecipeId} not found.");

        if (!await _userRepository.ExistsAsync(like.UserId))
            throw new KeyNotFoundException($"User with ID {like.UserId} not found.");

        if (await _likeRepository.ExistsAsync(like.RecipeId, like.UserId))
            throw new InvalidOperationException("User has already liked this recipe.");

        like.CreatedAt = DateTime.UtcNow;
        await _likeRepository.AddAsync(like);
        return MapToDTO(like);
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
            RecipeName = like.Recipe?.Name,
            CreatedAt = like.CreatedAt
        };
    }
}
