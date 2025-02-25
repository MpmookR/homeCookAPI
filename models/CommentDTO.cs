using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class CommentDTO
    {
        public int CommentId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Instead of exposing full User model
        public string UserId { get; set; }
        public string UserName { get; set; }

        public int RecipeId { get; set; }

        public int? ParentCommentId {get; set;}
    }
}
