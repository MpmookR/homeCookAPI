namespace homeCookAPI.Models
{  
    public class Like{
        public int LikeId { get; set; }

        public ApplicationUser User { get; set; }
        public string UserId { get; set; } // Foreign Key (Identity uses string ID)

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

}