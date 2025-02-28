using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class RecipeDTO
    {
        public int RecipeId { get; set; }

        [Required(ErrorMessage = "Recipe name is required.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Recipe name must be between 3 and 150 characters.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public required string Category { get; set; }

        [Required(ErrorMessage = "Intro is required.")]
        [StringLength(300, ErrorMessage = "Intro must be a maximum of 300 characters.")]
        public required string Intro { get; set; }

        [Required(ErrorMessage = "Ingredients are required.")]
        public required string Ingredients { get; set; }

        [Required(ErrorMessage = "Instructions are required.")]
        public required string HowTo { get; set; }

        [Url(ErrorMessage = "Invalid image URL format.")]
        public string? Image { get; set; }

        public DateTime CreateDate { get; set; }

        // User Details (Expose limited user info)
        [Required(ErrorMessage = "User ID is required.")]
        public required string UserId { get; set; }
        
        // Includes the username without exposing full user entity
        public string UserName { get; set; } = string.Empty; 
        // Collections of related entities (DTO versions)
        public List<CommentDTO> Comments { get; set; } = new();
        public List<RecipeRatingDTO> Ratings { get; set; } = new();
        public List<LikeDTO> Likes { get; set; } = new();
        public List<SavedRecipeDTO> SavedRecipes { get; set; } = new();
    }
}
