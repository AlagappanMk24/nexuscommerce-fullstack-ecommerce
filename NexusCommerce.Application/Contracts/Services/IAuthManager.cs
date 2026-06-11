using Microsoft.AspNetCore.Identity;
using NexusCommerce.Application.DTOs.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexusCommerce.Application.Contracts.Services
{
    /// <summary>
    /// Interface for authentication management operations
    /// </summary>
    public interface IAuthManager
    {
        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="dto">The registration data transfer object containing user details</param>
        /// <returns>A collection of identity errors if registration fails; null if successful</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when dto is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when registration fails due to internal error</exception>
        Task<IEnumerable<IdentityError>?> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Authenticates a user and generates a JWT token
        /// </summary>
        /// <param name="dto">The login credentials containing email and password</param>
        /// <returns>JWT token if authentication is successful; null if credentials are invalid or account is locked/deleted</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when dto is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when login fails due to internal error</exception>
        Task<LoginResponseDto> LoginAsync(LoginDto dto);

        /// <summary>
        /// Validates OTP and completes authentication
        /// </summary>
        /// <param name="dto">The OTP validation data</param>
        /// <returns>JWT token if validation successful</returns>
        Task<string?> ValidateOtpAndCompleteLoginAsync(ValidateOtpDto dto);

        /// <summary>
        /// Resends OTP to user's email
        /// </summary>
        /// <param name="dto">The resend OTP request data</param>
        /// <returns>True if OTP was resent successfully</returns>
        Task<bool> ResendOtpAsync(ResendOtpDto dto);

        /// <summary>
        /// Sends a password reset link to the user's email
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="resetBaseUrl">The base URL for the reset password page (without query parameters)</param>
        /// <returns>True if the reset email was sent successfully; false if user not found</returns>
        /// <exception cref="System.ArgumentException">Thrown when email or resetBaseUrl is null or empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when email sending fails</exception>
        Task<bool> ForgotPasswordAsync(string email, string resetBaseUrl);

        /// <summary>
        /// Resets the user's password using a valid reset token
        /// </summary>
        /// <param name="dto">The reset password data containing email, token, and new password</param>
        /// <returns>True if password reset was successful; false if user not found or token is invalid</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when dto is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when password reset fails due to internal error</exception>
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}