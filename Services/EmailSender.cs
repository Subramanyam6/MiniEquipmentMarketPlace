using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using PostmarkDotNet;
using System.Threading.Tasks;

namespace MiniEquipmentMarketplace.Services
{
    // Holds Postmark settings from appsettings.json
    public class EmailSettings
    {
        public string ServerToken { get; set; } = default!;
        public string FromName    { get; set; } = default!;
        public string FromEmail   { get; set; } = default!;
    }

    // Implements ASP.NET Core's IEmailSender using Postmark API
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public EmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new PostmarkClient(_settings.ServerToken);

            var message = new PostmarkMessage
            {
                From = string.IsNullOrWhiteSpace(_settings.FromName)
                    ? _settings.FromEmail
                    : $"\"{_settings.FromName}\" <{_settings.FromEmail}>",
                To = email,
                Subject = subject,
                HtmlBody = htmlMessage,
            };

            var response = await client.SendMessageAsync(message);
            if (response.Status != PostmarkStatus.Success)
            {
                throw new System.Exception($"Failed to send email via Postmark: {response.Message}");
            }
        }
    }
}
