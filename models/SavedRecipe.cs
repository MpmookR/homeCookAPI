using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace homeCookAPI.Models
{
    public class SavedRecipe
    {
        [Key]
        public int SavedRecipeId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public required string UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Recipe ID is required.")]
        public required int RecipeId { get; set; }

        [JsonIgnore]
        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
