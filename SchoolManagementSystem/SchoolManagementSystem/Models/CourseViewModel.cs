using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Course Name")]
        public string Name { get; set; }

        [Display(Name = "Subjects")]
        public List<int> SelectedSubjectIds { get; set; } = new();

        public IEnumerable<SelectListItem>? Subjects { get; set; }
    }
}
