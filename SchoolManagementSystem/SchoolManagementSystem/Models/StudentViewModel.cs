using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class StudentViewModel
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student photo is required")]
        [Display(Name = "Student photo")]
        public IFormFile? PhotoFile { get; set; }

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
