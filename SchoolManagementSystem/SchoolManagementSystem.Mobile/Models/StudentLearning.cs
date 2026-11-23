using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagementSystem.Mobile.Models
{
    public class StudentLearning
    {
        public string FullName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        public decimal MaxAbsencePercentage { get; set; }
        public List<StudentSubject> Subjects { get; set; } = [];
    }
}
