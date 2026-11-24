using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagementSystem.Mobile.Models
{
    public class StudentSubject
    {
        public int EnrollmentId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public int TotalHours { get; set; }

        public int AbsenceCount { get; set; }

        public decimal? FinalGrade { get; set; }
    }
}
