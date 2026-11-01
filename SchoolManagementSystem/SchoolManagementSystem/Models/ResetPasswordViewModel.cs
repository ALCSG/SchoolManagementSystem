using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
