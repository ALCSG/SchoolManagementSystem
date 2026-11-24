using SchoolManagementSystem.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models.API
{
    public class UpdateStudentProfile
    {
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string? PhoneNumber { get; set; }

        [ImageFileHelper(2)]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}
