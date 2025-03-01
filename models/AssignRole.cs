using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class AssignRole
    {
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        public string RoleName { get; set; }
    }
}
