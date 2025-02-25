using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class RecipeDTO
    {
        public int RecipeId { get; set; }
        
        [Required(ErrorMessage = "Recipe name is required.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Recipe name must be between 3 and 150 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Intro is required.")]
        [StringLength(300, ErrorMessage = "Intro must be a maximum of 300 characters.")]
        public string Intro { get; set; }

        [Required(ErrorMessage = "Ingredients are required.")]
        public string Ingredients { get; set; }
        
        [Required(ErrorMessage = "Instructions are required.")]
        public string HowTo { get; set; }
        
        public string Image { get; set; }
        public DateTime CreateDate { get; set; }

        // User Details (Owner of the Recipe)
        public string UserId { get; set; }
        public string UserName { get; set; } // Display only the User's Name

        // Include Comments, Ratings, Likes, and Saved Recipes
        public List<CommentDTO> Comments { get; set; } = new();
        public List<RecipeRatingDTO> Ratings { get; set; } = new();
        public List<LikeDTO> Likes { get; set; } = new();
        public List<SavedRecipeDTO> SavedRecipes { get; set; } = new();
    }
}
