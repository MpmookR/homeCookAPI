using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class UserDTO
    {
        public string Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime JoinDate { get; set; }
    }
}
