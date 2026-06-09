using Microsoft.AspNetCore.Identity;
using NexusCommerce.Application.DTOs.Auth;

namespace NexusCommerce.Application.Contracts.Services
{
    public interface IAuthManager
    {
        Task<string?> LoginAsync(LoginDto dto);
        Task<IEnumerable<IdentityError>?> RegisterAsync(RegisterDto dto);
        Task<bool> ForgotPasswordAsync(string email, string resetBaseUrl);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
