namespace SchoolManagementSystem.Models.API
{
    public class MyLearningResponse
    {
        public string FullName { get; set; } = string.Empty;

        public string? CourseName { get; set; }

        public string? ClassGroupName { get; set; }

        public bool IsArchived { get; set; }

        public int MaxAbsencePercentage { get; set; }

        public List<MyLearningSubjectResponse> Subjects { get; set; } = new();
    }

    
}