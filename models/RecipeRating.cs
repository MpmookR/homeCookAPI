namespace homeCookAPI.Models
{
    public class RecipeRating
    {
        public int Id { get; set; }
        public int Rating { get; set; } // e.g., 1-5 stars

        public ApplicationUser User { get; set; }
        public string UserId { get; set; } // Foreign Key

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}