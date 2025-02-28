using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace homeCookAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
        public required string FullName { get; set; }

        [Url(ErrorMessage = "Invalid profile image URL format")]
        public string? ProfileImage { get; set; }

        [Required(ErrorMessage = "Join date is required")]
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        // represent the relationship m-m
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<RecipeRating> RecipeRatings { get; set; } = new List<RecipeRating>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<SavedRecipe> SavedRecipes { get; set; } = new List<SavedRecipe>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}

