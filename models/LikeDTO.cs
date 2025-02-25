using System;

namespace homeCookAPI.Models
{
    public class LikeDTO
    {
        public int LikeId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // Display only User's Name
        public int RecipeId { get; set; }
        public string RecipeName { get; set; } // Display only Recipe Name
        public DateTime CreatedAt { get; set; }
    }
}
