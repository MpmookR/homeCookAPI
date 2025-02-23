using System.Text.Json.Serialization;

namespace homeCookAPI.Models
{  
    public class Like
    {
        public int LikeId { get; set; }
        
        [JsonIgnore] public ApplicationUser? User { get; set; }
        public string? UserId { get; set; } // Foreign Key (Identity uses string ID)

        public int RecipeId { get; set; }  
        [JsonIgnore] public Recipe? Recipe { get; set; }  //[JsonIgnore] here to prevent loops

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
