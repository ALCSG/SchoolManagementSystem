using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class ClassGroupViewModel
    {
        public int ClassGroupId { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public IEnumerable<SelectListItem>? CoursesList { get; set; }

        public int CourseId { get; set; }
    }
}
