namespace SchoolManagementSystem.Models.API
{
    public class StudentProfileResponse
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
