using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace homeCookAPI.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 500 characters.")]
        public required string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "User ID is required.")]
        public required string UserId { get; set; } 

        [JsonIgnore]
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Recipe ID is required.")]
        public required int RecipeId { get; set; }

        [JsonIgnore]
        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        // Self-referencing foreign key for threaded comments
        public int? ParentCommentId { get; set; }

        [JsonIgnore]
        [ForeignKey("ParentCommentId")]
        public Comment? ParentComment { get; set; }

        [JsonIgnore]
        public List<Comment> Replies { get; set; } = new();
    }
}
