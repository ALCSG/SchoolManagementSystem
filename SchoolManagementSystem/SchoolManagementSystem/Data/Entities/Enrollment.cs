using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Data.Entities
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey(nameof(Subject))]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        [Range(0, 365)]
        public int AbsenceCount { get; set; }

        [Range(0, 20)]
        public double? FinalGrade { get; set; }

        [NotMapped]
        public bool IsApproved => FinalGrade.HasValue && FinalGrade.Value >= 9.5;
    }
}
