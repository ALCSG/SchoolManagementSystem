using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
