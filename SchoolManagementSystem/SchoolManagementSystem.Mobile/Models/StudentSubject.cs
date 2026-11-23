using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagementSystem.Mobile.Models
{
    public class StudentSubject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public decimal CurrentABsencePercentage { get; set; }
        public bool IsArchived { get; set; }
    }
}
