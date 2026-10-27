using Microsoft.AspNetCore.Http;
using SchoolManagementSystem.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class ProfileViewModel
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        public string? ProfilePicturePath { get; set; }

        [Display(Name = "Profile picture")]
        [MaxFileSizeHelper(2)]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}