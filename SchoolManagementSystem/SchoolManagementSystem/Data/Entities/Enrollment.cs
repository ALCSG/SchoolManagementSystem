using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Data.Entities
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int AbsenceCount { get; set; }

        public double? FinalGrade { get; set; }

        public bool IsApproved => FinalGrade.HasValue && FinalGrade.Value >= 9.5;
    }
}
