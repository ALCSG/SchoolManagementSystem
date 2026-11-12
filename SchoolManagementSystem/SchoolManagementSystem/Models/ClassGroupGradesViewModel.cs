using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Models
{
    public class ClassGroupGradesViewModel
    {
        public ClassGroup ClassGroup { get; set; } = null!;

        public int MaxAbsencePercentage { get; set; }
    }
}