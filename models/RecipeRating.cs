using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace homeCookAPI.Models
{
    public class RecipeRating
    {
        [Key]
        public int RecipeRatingId { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

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
    }
}
