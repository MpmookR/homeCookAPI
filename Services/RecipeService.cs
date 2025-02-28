using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeCookAPI.Models;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IUserRepository _userRepository;

    public RecipeService(IRecipeRepository recipeRepository, IUserRepository userRepository)
    {
        _recipeRepository = recipeRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<RecipeDTO>> GetAllRecipesAsync()
    {
        var recipes = await _recipeRepository.GetAllAsync();
        return recipes.Select(MapToDTO);
    }

    public async Task<RecipeDTO> GetRecipeByIdAsync(int id)
    {
        var recipe = await _recipeRepository.GetByIdAsync(id);
        if (recipe == null) throw new KeyNotFoundException($"Recipe with ID {id} not found.");
        return MapToDTO(recipe);
    }

    public async Task<RecipeDTO> AddRecipeAsync(RecipeDTO recipeDTO)
    {
        if (!await _userRepository.ExistsAsync(recipeDTO.UserId))
            throw new KeyNotFoundException($"User with ID {recipeDTO.UserId} not found.");

        var recipe = new Recipe
        {
            Name = recipeDTO.Name,
            Category = recipeDTO.Category,
            Intro = recipeDTO.Intro,
            Ingredients = recipeDTO.Ingredients,
            HowTo = recipeDTO.HowTo,
            Image = recipeDTO.Image,
            CreateDate = DateTime.UtcNow,
            UserId = recipeDTO.UserId
        };

        await _recipeRepository.AddAsync(recipe);
        return MapToDTO(recipe);
    }

    public async Task<RecipeDTO> UpdateRecipeAsync(int id, RecipeDTO recipeDTO, string userId)
    {
        var recipe = await _recipeRepository.GetByIdAsync(id);
        if (recipe == null) throw new KeyNotFoundException($"Recipe with ID {id} not found.");
        if (recipe.UserId != userId) throw new UnauthorizedAccessException("You are not authorized to edit this recipe.");

        recipe.Name = recipeDTO.Name ?? recipe.Name;
        recipe.Category = recipeDTO.Category ?? recipe.Category;
        recipe.Intro = recipeDTO.Intro ?? recipe.Intro;
        recipe.Ingredients = recipeDTO.Ingredients ?? recipe.Ingredients;
        recipe.HowTo = recipeDTO.HowTo ?? recipe.HowTo;
        recipe.Image = recipeDTO.Image ?? recipe.Image;

        await _recipeRepository.UpdateAsync(recipe);
        return MapToDTO(recipe);
    }

    public async Task<bool> DeleteRecipeAsync(int id, string userId, bool isAdmin)
    {
        var recipe = await _recipeRepository.GetByIdAsync(id);
        if (recipe == null) throw new KeyNotFoundException($"Recipe with ID {id} not found.");
        if (!isAdmin && recipe.UserId != userId) throw new UnauthorizedAccessException("You are not authorized to delete this recipe.");

        await _recipeRepository.DeleteAsync(recipe);
        return true;
    }

    private static RecipeDTO MapToDTO(Recipe recipe)
    {
        return new RecipeDTO
        {
            RecipeId = recipe.RecipeId,
            Name = recipe.Name,
            Category = recipe.Category,
            Intro = recipe.Intro,
            Ingredients = recipe.Ingredients,
            HowTo = recipe.HowTo,
            Image = recipe.Image,
            CreateDate = recipe.CreateDate,
            UserId = recipe.UserId,
            UserName = recipe.User?.FullName ?? "",

            // Map related entities for all the entities to show on get
            Comments = recipe.Comments.Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                UserName = c.User?.FullName,
                RecipeId = c.RecipeId
            }).ToList(),

            Likes = recipe.Likes.Select(l => new LikeDTO
            {
                LikeId = l.LikeId,
                UserId = l.UserId,
                UserName = l.User?.FullName,
                RecipeId = l.RecipeId
            }).ToList(),

            SavedRecipes = recipe.SavedRecipes.Select(s => new SavedRecipeDTO
            {
                SavedRecipeId = s.SavedRecipeId,
                UserId = s.UserId,
                UserName = s.User?.FullName,
                RecipeId = s.RecipeId
            }).ToList(),

            Ratings = recipe.Ratings.Select(r => new RecipeRatingDTO
            {
                RecipeRatingId = r.RecipeRatingId,
                UserId = r.UserId,
                UserName = r.User?.FullName,
                RecipeId = r.RecipeId,
                Rating = r.Rating
            }).ToList()

        };
    }
}
