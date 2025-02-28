using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace homeCookAPI.Models
{   
    public class Recipe
    {
        [Key]
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

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // Foreign Key for ApplicationUser
        public required string UserId { get; set; }

        // Navigation Property (EF Core Relationship)
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<RecipeRating> Ratings { get; set; } = new List<RecipeRating>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<SavedRecipe> SavedRecipes { get; set; } = new List<SavedRecipe>();
    }
}