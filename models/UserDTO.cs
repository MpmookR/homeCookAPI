using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class UserDTO
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3,ErrorMessage = "Full name must be between 3 and 100 characters" )]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress( ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        public DateTime JoinDate { get; set; }
    }
}
