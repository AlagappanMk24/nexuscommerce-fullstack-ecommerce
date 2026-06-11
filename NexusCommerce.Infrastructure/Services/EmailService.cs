using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Domain.Settings;

namespace NexusCommerce.Infrastructure.Services
{
    /// <summary>
    /// Service for sending emails using SMTP protocol
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EmailService"/> class
    /// </remarks>
    /// <param name="settings">Email configuration settings</param>
    /// <exception cref="ArgumentNullException">Thrown when settings is null</exception>
    public class EmailService(IOptions<EmailSettings> settings) : IEmailService
    {
        private readonly EmailSettings _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        /// <inheritdoc/>
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email cannot be null or empty", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Email subject cannot be null or empty", nameof(subject));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Email body cannot be null or empty", nameof(body));

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail.Trim()));
                message.Subject = subject.Trim();
                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using var smtp = new SmtpClient();
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send email to {toEmail}", ex);
            }
        }
    }
}