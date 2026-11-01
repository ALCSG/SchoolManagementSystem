namespace SchoolManagementSystem.Helpers
{
    public interface IMailHelper
    {
        Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body);
    }
}
