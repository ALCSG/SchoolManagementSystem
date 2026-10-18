using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class EnrollmentViewModel
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }

        [Display(Name = "Student")]
        public string? StudentName { get; set; }

        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        [Display(Name = "Subject")]
        public int SubjectName { get; set; }

        [Display(Name = "Absences")]
        [Range(0, 365)]
        public int AbsenceCount { get; set; }

        [Display(Name = "Final Grade")]
        [Range(0, 20)]
        public double? FinalGrade { get; set; }

        public IEnumerable<SelectListItem>? SubjectsList { get; set; }
    }
}
