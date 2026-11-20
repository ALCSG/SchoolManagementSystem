namespace SchoolManagementSystem.Models.API
{
    public class MyLearningSubjectResponse
    {
        public int EnrollmentId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public int TotalHours { get; set; }

        public int AbsenceCount { get; set; }

        public double? FinalGrade { get; set; }
    }
}
