using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class AssignRoleDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "RoleName is required.")]
        [RegularExpression("^(SuperAdmin|Admin|User)$", ErrorMessage = "Invalid role name.")]
        public string RoleName { get; set; }
    }
}
