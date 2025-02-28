using System;
using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class SavedRecipeDTO
    {
        public int SavedRecipeId { get; set; }

        public string? UserId { get; set; }// will be assigned from authentication

        public string? UserName { get; set; } // Optional, only for display

        [Required(ErrorMessage = "Recipe ID is required.")]
        public required int RecipeId { get; set; }

        public string? RecipeName { get; set; } // Display only Recipe Name

        public DateTime CreatedAt { get; set; }
    }
}
