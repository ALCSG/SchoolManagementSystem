using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Runtime;

namespace SchoolManagementSystem.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly MailSettings _settings;
        private readonly ILogger<MailHelper> _logger;

        public MailHelper(IOptions<MailSettings> options, ILogger<MailHelper> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = builder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.UserName, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send e-mail");
                return false;
            }
        }
    }
}
