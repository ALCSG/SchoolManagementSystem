using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class StudentViewModel
    {
        public int StudentId { get; set; }

        [Display(Name = "Student photo")]
        [MaxFileSizeHelper(2)]
        public IFormFile? PhotoFile { get; set; }

        public string? CurrentPhotoPath { get; set; }

        [Required]
        [Display(Name = "Class Group")]
        public int ClassGroupId { get; set; }

        [Required, MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public IEnumerable<SelectListItem>? ClassGroups { get; set; }
    }
}
