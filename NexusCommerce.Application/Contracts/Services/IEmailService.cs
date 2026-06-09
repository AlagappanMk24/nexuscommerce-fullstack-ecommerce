namespace NexusCommerce.Application.Contracts.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
}