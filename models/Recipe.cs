namespace homeCookAPI.Models
{   
    public class Recipe
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Intro { get; set; }
        public string Ingredients { get; set; } 
        public string HowTo { get; set; } 
        public string Image { get; set; }
        
        public DateTime CreateDate { get; set; } = DateTime.UtcNow; // Default to current time
        
        public ApplicationUser? User { get; set; }
        public string? UserId { get; set; } // Foreign Key (IdentityUser uses string ID)

        // Navigation properties (One Recipe has many Comments)
        //It allows EF Core to retrieve all comments associated with a specific recipe.

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<RecipeRating> Ratings { get; set; } = new List<RecipeRating>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<SavedRecipe> SavedRecipes { get; set; } = new List<SavedRecipe>();
    }
}