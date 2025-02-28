using System;
using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class LikeDTO
    {
        public int LikeId { get; set; }

        public string? UserId { get; set; }

        public string? UserName { get; set; }

        [Required(ErrorMessage = "Recipe ID is required.")]
        public required int RecipeId { get; set; }

        public string? RecipeName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

