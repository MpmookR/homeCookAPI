using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class UserDTO
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }

        public DateTime JoinDate { get; set; }
        public required List<string> Roles { get; set; }

        public string? ProfileImage { get; set; }
        public int TotalRecipes { get; set; }
        public int TotalLikes { get; set; }
        public int TotalSavedRecipes { get; set; }

    }
}
