using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Application.DTOs.Auth;
using NexusCommerce.Domain.Entities;
using NexusCommerce.Domain.Entities.Identity;

namespace NexusCommerce.Application.Services
{
    /// <summary>
    /// Manages authentication operations including user registration, login, password reset, and email verification
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AuthManager"/> class
    /// </remarks>
    /// <param name="userManager">The user manager for handling user operations</param>
    /// <param name="configuration">Application configuration settings</param>
    /// <param name="unitOfWork">Unit of work for database operations</param>
    /// <param name="emailService">Email service for sending notifications</param>
    /// <param name="jwtService">JWT service for token generation</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
    public class AuthManager(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IJwtService jwtService,
        IOtpService otpService,
        ILogger<AuthManager> logger) : IAuthManager
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly IJwtService _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        private readonly IOtpService _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
        private readonly ILogger<AuthManager> _logger = logger;

        /// <inheritdoc/>
        public async Task<IEnumerable<IdentityError>?> RegisterAsync(RegisterDto dto)
        {
            try
            {
                if (dto == null)
                {
                    throw new ArgumentNullException(nameof(dto));
                }

                var user = new ApplicationUser
                {
                    FirstName = dto.FirstName?.Trim(),
                    LastName = dto.LastName?.Trim(),
                    Email = dto.Email?.Trim().ToLowerInvariant(),
                    UserName = dto.Email?.Trim().ToLowerInvariant(),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                    return result.Errors;

                var accountType = char.ToUpper(dto.AccountType[0]) + dto.AccountType.Substring(1).ToLower();
                await _userManager.AddToRoleAsync(user, accountType);

                if (accountType == "Customer")
                {
                    _unitOfWork.CustomerRepository.Add(new Customer
                    {
                        UserId = user.Id,
                        Address = dto.Address?.Trim()
                    });
                }
                else if (accountType == "Seller")
                {
                    _unitOfWork.SellerRepository.Add(new Seller
                    {
                        UserId = user.Id,
                        StoreName = dto.StoreName?.Trim()
                    });
                }

                await _unitOfWork.SaveAsync();
                return null;
            }
            catch (Exception ex)
            {
                // Log exception here using ILogger
                throw new InvalidOperationException("An error occurred during user registration.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

                var user = await _userManager.FindByEmailAsync(dto.Email?.Trim().ToLowerInvariant());

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", dto.Email);
                    return new LoginResponseDto
                    {
                        Message = "Invalid email or password."
                    };
                }

                var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);

                if (!isPasswordCorrect)
                {
                    _logger.LogWarning("Login failed: Invalid password for email {Email}", dto.Email);
                    return new LoginResponseDto
                    {
                        Message = "Invalid email or password."
                    };
                }

                if (user.IsDeleted || !user.IsActive)
                {
                    _logger.LogWarning("Login failed: Account inactive or deleted for email {Email}", dto.Email);
                    return new LoginResponseDto
                    {
                        Message = "Your account is inactive. Please contact support."
                    };
                }

                // Generate and send OTP
                var otpResult = await _otpService.GenerateAndSendOtpAsync(user);

                if (!otpResult.Success)
                {
                    _logger.LogError("Failed to generate OTP for user: {Email}", dto.Email);
                    return new LoginResponseDto
                    {
                        Message = "Failed to send verification code. Please try again."
                    };
                }

                _logger.LogInformation("OTP sent successfully for user: {Email}", dto.Email);

                return new LoginResponseDto
                {
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    OtpToken = otpResult.OtpToken,
                    OtpIdentifier = otpResult.OtpIdentifier,
                    Message = $"Verification code sent to {MaskEmail(user.Email)}. Please check your email."
                };
            }
            catch (Exception ex)
            {
                // Log exception here using ILogger
                throw new InvalidOperationException("An error occurred during user login.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string?> ValidateOtpAndCompleteLoginAsync(ValidateOtpDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                _logger.LogInformation("OTP validation attempt for identifier: {OtpIdentifier}", dto.OtpIdentifier);

                // Validate OTP
                var isValid = await _otpService.ValidateOtpAsync(dto);

                if (!isValid)
                {
                    _logger.LogWarning("OTP validation failed for identifier: {OtpIdentifier}", dto.OtpIdentifier);
                    return null;
                }

                // Get user
                var user = await _userManager.FindByIdAsync(dto.UserId);

                if (user == null)
                {
                    _logger.LogWarning("User not found after OTP validation for ID: {UserId}", dto.UserId);
                    return null;
                }

                // Generate JWT token
                var jwtToken = await _jwtService.GenerateJwtTokenAsync(user);

                _logger.LogInformation("Login completed successfully for user: {Email}", user.Email);

                return jwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP validation for identifier: {OtpIdentifier}", dto?.OtpIdentifier);
                throw new InvalidOperationException("An error occurred during OTP validation.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ResendOtpAsync(ResendOtpDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                _logger.LogInformation("Resending OTP for identifier: {OtpIdentifier}", dto.OtpIdentifier);

                return await _otpService.ResendOtpAsync(dto.OtpIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP for identifier: {OtpIdentifier}", dto?.OtpIdentifier);
                throw new InvalidOperationException("An error occurred while resending OTP.", ex);
            }
        }
        /// <inheritdoc/>
        public async Task<bool> ForgotPasswordAsync(string email, string resetBaseUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email cannot be null or empty", nameof(email));

                if (string.IsNullOrWhiteSpace(resetBaseUrl))
                    throw new ArgumentException("Reset base URL cannot be null or empty", nameof(resetBaseUrl));

                var user = await _userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());

                if (user == null)
                    return false;

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Uri.EscapeDataString(token);
                var resetLink = $"{resetBaseUrl.TrimEnd('/')}?email={email}&token={encodedToken}";

                var emailBody = GeneratePasswordResetEmailBody(user, resetLink);
                await _emailService.SendAsync(email, "Reset Your NexusCommerce Password", emailBody);

                _logger.LogInformation("Password reset email sent to: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", email);
                throw new InvalidOperationException("An error occurred while processing forgot password request.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var user = await _userManager.FindByEmailAsync(dto.Email?.Trim().ToLowerInvariant());

                if (user == null)
                    return false;

                var decodedToken = Uri.UnescapeDataString(dto.Token);
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset successful for user: {Email}", dto.Email);
                }
                else
                {
                    _logger.LogWarning("Password reset failed for user: {Email}", dto.Email);
                }
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reset password for email: {Email}", dto?.Email);
                throw new InvalidOperationException("An error occurred while resetting password.", ex);
            }
        }

        /// <summary>
        /// Masks email address for display
        /// </summary>
        private static string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;

            var localPart = parts[0];
            var domain = parts[1];

            if (localPart.Length <= 2)
                return $"{localPart[0]}***@{domain}";

            return $"{localPart[0]}***{localPart[^1]}@{domain}";
        }

        /// <summary>
        /// Generates an HTML email body for password reset notification
        /// </summary>
        /// <param name="user">The user requesting password reset</param>
        /// <param name="resetLink">The password reset link</param>
        /// <returns>HTML formatted email body</returns>
        private static string GeneratePasswordResetEmailBody(ApplicationUser user, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Password Reset Request</title>
                    <style>
                        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f5; }}
                        .container {{ max-width: 560px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; }}
                        .header h1 {{ color: white; margin: 0; font-size: 28px; font-weight: 600; }}
                        .content {{ padding: 40px 30px; }}
                        .button {{ display: inline-block; padding: 12px 32px; background: linear-gradient(135deg, #667eea, #764ba2); color: white; text-decoration: none; border-radius: 8px; margin: 20px 0; font-weight: 600; }}
                        .button:hover {{ opacity: 0.9; transform: translateY(-2px); }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #6c757d; background-color: #f9fafb; border-top: 1px solid #e5e7eb; }}
                        .note {{ background-color: #fef3c7; padding: 12px; border-radius: 8px; margin: 20px 0; font-size: 13px; border-left: 4px solid #f59e0b; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Reset Your Password</h1>
                        </div>
                        <div class='content'>
                            <p>Hello <strong>{System.Security.SecurityElement.Escape(user.FirstName)} {System.Security.SecurityElement.Escape(user.LastName)}</strong>,</p>
                            <p>We received a request to reset the password for your NexusCommerce account.</p>
                            <p>Click the button below to create a new password:</p>
                            <div style='text-align: center;'>
                                <a href='{resetLink}' class='button'>Reset Password</a>
                            </div>
                            <div class='note'>
                                <strong>⚠️ Security Note:</strong> This link will expire in 1 hour for your security.
                            </div>
                            <p>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
                            <p><strong>For security reasons, never share this link with anyone.</strong></p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 NexusCommerce. All rights reserved.</p>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}
