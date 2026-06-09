using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Domain.Settings;

namespace NexusCommerce.Infrastructure.Services
{
    public class EmailService(IOptions<EmailSettings> settings) : IEmailService
    {
        private readonly EmailSettings _settings = settings.Value;
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}