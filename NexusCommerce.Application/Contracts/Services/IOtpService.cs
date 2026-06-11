using NexusCommerce.Application.DTOs.Auth;
using NexusCommerce.Domain.Entities.Identity;

namespace NexusCommerce.Application.Contracts.Services
{
    /// <summary>
    /// Service interface for OTP (One-Time Password) operations
    /// </summary>
    public interface IOtpService
    {
        /// <summary>
        /// Generates and sends an OTP to the user's email for two-factor authentication
        /// </summary>
        /// <param name="user">The application user</param>
        /// <returns>OTP token and identifier for validation</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when user is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when OTP generation or email sending fails</exception>
        Task<OtpGenerationResult> GenerateAndSendOtpAsync(ApplicationUser user);

        /// <summary>
        /// Validates the provided OTP code
        /// </summary>
        /// <param name="dto">The OTP validation data</param>
        /// <returns>True if OTP is valid; false otherwise</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when dto is null</exception>
        Task<bool> ValidateOtpAsync(ValidateOtpDto dto);

        /// <summary>
        /// Resends a new OTP to the user's email
        /// </summary>
        /// <param name="otpIdentifier">The unique identifier for the OTP session</param>
        /// <returns>True if OTP was resent successfully; false otherwise</returns>
        /// <exception cref="System.ArgumentException">Thrown when otpIdentifier is null or empty</exception>
        Task<bool> ResendOtpAsync(string otpIdentifier);

        /// <summary>
        /// Generates a temporary JWT token for OTP validation session
        /// </summary>
        /// <param name="user">The application user</param>
        /// <param name="otpIdentifier">Unique identifier for the OTP session</param>
        /// <returns>JWT token string for OTP validation</returns>
        string GenerateOtpToken(ApplicationUser user, string otpIdentifier);

        /// <summary>
        /// Validates the OTP token and extracts user information
        /// </summary>
        /// <param name="otpToken">The OTP token to validate</param>
        /// <returns>Claims principal if valid; null otherwise</returns>
        System.Security.Claims.ClaimsPrincipal? ValidateOtpToken(string otpToken);
    }
}