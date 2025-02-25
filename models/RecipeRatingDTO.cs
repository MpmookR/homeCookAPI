using System;

namespace homeCookAPI.Models
{
    public class RecipeRatingDTO
    {
        public int RecipeRatingId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // Display only User's Name
        public int RecipeId { get; set; }
        public int Rating { get; set; } // Rating value (e.g., 1-5 stars)
    }
}
