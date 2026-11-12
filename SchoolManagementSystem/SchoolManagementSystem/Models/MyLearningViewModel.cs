using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Models
{
    public class MyLearningViewModel
    {
        public Student Student { get; set; } = null!;

        public int MaxAbsencePercentage { get; set; }
    }
}