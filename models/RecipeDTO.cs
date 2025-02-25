using System;
using System.Collections.Generic;

namespace homeCookAPI.Models
{
    public class RecipeDTO
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Intro { get; set; }
        public string Ingredients { get; set; }
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
