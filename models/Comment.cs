using System.Text.Json.Serialization;

namespace homeCookAPI.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
        public string? UserId { get; set; } // Foreign Key (string because Identity uses string IDs)
        
        [JsonIgnore]
        public Recipe? Recipe { get; set; }
        public int RecipeId { get; set; }


        // Self-referencing foreign key for threading
        public int? ParentCommentId { get; set; }
        [JsonIgnore]        
        public Comment? ParentComment { get; set; }
        [JsonIgnore]
        public List<Comment> Replies { get; set; } = new();
    }
}