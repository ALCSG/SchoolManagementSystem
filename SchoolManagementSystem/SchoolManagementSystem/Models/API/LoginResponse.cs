namespace SchoolManagementSystem.Models.API
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
