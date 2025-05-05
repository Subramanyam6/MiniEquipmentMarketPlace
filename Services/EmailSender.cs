using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Threading.Tasks;

namespace MiniEquipmentMarketplace.Services
{
    // Holds your SMTP settings from appsettings.json
    public class EmailSettings
    {
        public string Host      { get; set; } = default!;
        public int    Port      { get; set; }
        public string UserName  { get; set; } = default!;
        public string Password  { get; set; } = default!;
        public string FromName  { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
    }

    // Implements ASP.NET Core's IEmailSender
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public EmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            using var client = new SmtpClient();
            // StartTLS on port 587, required by Office 365/Outlook
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.UserName, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
