using System.ComponentModel.DataAnnotations;

namespace homeCookAPI.Models
{
    public class ReportUser
    {
        [Required(ErrorMessage = "User ID is required")]
        public required string UserId { get; set; }
    }
}
