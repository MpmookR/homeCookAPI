namespace homeCookAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
        public string UserId { get; set; } // Foreign Key (string because Identity uses string IDs)

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }


        // Self-referencing foreign key for threading
        public int? ParentCommentId { get; set; }
        public Comment ParentComment { get; set; }
        public List<Comment> Replies { get; set; } = new();
    }
}