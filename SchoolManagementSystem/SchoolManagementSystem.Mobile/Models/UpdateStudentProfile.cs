using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagementSystem.Mobile.Models
{
    public class UpdateStudentProfile
    {
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? PhotoUrl { get; set; }
    }
}
