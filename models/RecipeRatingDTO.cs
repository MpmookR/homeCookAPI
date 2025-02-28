using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class RecipeRatingDTO
    {
        public int RecipeRatingId { get; set; }

        public string? UserId { get; set; }

        public string? UserName { get; set; } 

        [Required(ErrorMessage = "Recipe ID is required.")]
        public required int RecipeId { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; } 
    }
}
