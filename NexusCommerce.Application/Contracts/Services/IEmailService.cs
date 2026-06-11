namespace NexusCommerce.Application.Contracts.Services
{
    /// <summary>
    /// Service interface for sending emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously
        /// </summary>
        /// <param name="toEmail">The recipient's email address</param>
        /// <param name="subject">The email subject line</param>
        /// <param name="body">The email body content (supports HTML)</param>
        /// <returns>A task representing the asynchronous email sending operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when toEmail, subject, or body is null or empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when email sending fails due to configuration or network issues</exception>
        Task SendAsync(string toEmail, string subject, string body);
    }
}