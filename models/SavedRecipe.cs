using System.Text.Json.Serialization;

namespace homeCookAPI.Models
{
    public class SavedRecipe
    {
        public int SavedRecipeId { get; set; }

        public string? UserId { get; set; }
        [JsonIgnore] public ApplicationUser? User { get; set; }

        public int RecipeId { get; set; }
        [JsonIgnore] public Recipe? Recipe { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
