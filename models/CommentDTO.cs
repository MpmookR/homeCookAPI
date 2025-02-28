using System;
using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class CommentDTO
    {
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 500 characters.")]
        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; } 
        public string? UserName { get; set; } 

        [Required(ErrorMessage = "Recipe ID is required.")]
        public required int RecipeId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
