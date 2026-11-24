namespace SchoolManagementSystem.Mobile.Models
{
    public class StudentProfile
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? PhotoUrl { get; set; }
    }
}