namespace homeCookAPI.Models
{
    public class SavedRecipe
    {
        public int Id { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
        public string UserId { get; set; } // Foreign Key

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }

}