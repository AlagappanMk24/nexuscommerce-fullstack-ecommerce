using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Application.DTOs.Auth;
using NexusCommerce.Domain.Entities.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NexusCommerce.Application.Services
{
    /// <summary>
    /// Service implementation for OTP (One-Time Password) operations
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="OtpService"/> class
    /// </remarks>
    public class OtpService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<OtpService> logger) : IOtpService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ILogger<OtpService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc/>
        public async Task<OtpGenerationResult> GenerateAndSendOtpAsync(ApplicationUser user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                // Generate 6-digit OTP
                var otp = GenerateOtp();
                var otpIdentifier = Guid.NewGuid().ToString();

                // Store OTP in user record
                user.TwoFactorCode = otp;
                user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);
                user.OtpIdentifier = otpIdentifier;

                await _userManager.UpdateAsync(user);

                // Generate temporary OTP token
                var otpToken = GenerateOtpToken(user, otpIdentifier);

                // Send OTP via email
                var emailBody = GenerateOtpEmailBody(user, otp);
                await _emailService.SendAsync(user.Email, "Your Verification Code for NexusCommerce", emailBody);

                _logger.LogInformation("OTP sent successfully to user: {Email}, Identifier: {OtpIdentifier}",
                    user.Email, otpIdentifier);

                return new OtpGenerationResult
                {
                    Success = true,
                    OtpToken = otpToken,
                    OtpIdentifier = otpIdentifier,
                    ExpiresIn = 300 // 5 minutes in seconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for user: {Email}", user?.Email);
                throw new InvalidOperationException("Failed to generate and send OTP.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateOtpAsync(ValidateOtpDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                _logger.LogInformation("Validating OTP for identifier: {OtpIdentifier}", dto.OtpIdentifier);

                // Validate OTP token
                var principal = ValidateOtpToken(dto.OtpToken);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid or expired OTP token for identifier: {OtpIdentifier}", dto.OtpIdentifier);
                    return false;
                }

                // Find user by OTP identifier
                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null || user.OtpIdentifier != dto.OtpIdentifier)
                {
                    _logger.LogWarning("User not found or identifier mismatch for: {OtpIdentifier}", dto.OtpIdentifier);
                    return false;
                }

                // Verify email matches
                var emailFromToken = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (emailFromToken != user.Email)
                {
                    _logger.LogWarning("Email mismatch for OTP validation: {OtpIdentifier}", dto.OtpIdentifier);
                    return false;
                }

                // Verify OTP code
                if (user.TwoFactorCode != dto.Otp)
                {
                    _logger.LogWarning("Invalid OTP code for identifier: {OtpIdentifier}", dto.OtpIdentifier);
                    return false;
                }

                // Check OTP expiry
                if (user.TwoFactorExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Expired OTP for identifier: {OtpIdentifier}", dto.OtpIdentifier);
                    return false;
                }

                // Clear OTP after successful validation
                user.TwoFactorCode = null;
                user.TwoFactorExpiry = null;
                user.OtpIdentifier = null;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("OTP validated successfully for identifier: {OtpIdentifier}", dto.OtpIdentifier);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP for identifier: {OtpIdentifier}", dto?.OtpIdentifier);
                throw new InvalidOperationException("Failed to validate OTP.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ResendOtpAsync(string otpIdentifier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(otpIdentifier))
                    throw new ArgumentException("OTP identifier cannot be null or empty", nameof(otpIdentifier));

                // Find user by OTP identifier
                var users = await _userManager.Users.ToListAsync();
                var user = users.FirstOrDefault(u => u.OtpIdentifier == otpIdentifier);

                if (user == null)
                {
                    _logger.LogWarning("User not found for OTP identifier: {OtpIdentifier}", otpIdentifier);
                    return false;
                }

                // Generate new OTP
                var newOtp = GenerateOtp();
                user.TwoFactorCode = newOtp;
                user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);

                await _userManager.UpdateAsync(user);

                // Send new OTP via email
                var emailBody = GenerateOtpEmailBody(user, newOtp);
                await _emailService.SendAsync(user.Email, "Your New Verification Code for NexusCommerce", emailBody);

                _logger.LogInformation("OTP resent successfully to user: {Email}, Identifier: {OtpIdentifier}",
                    user.Email, otpIdentifier);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP for identifier: {OtpIdentifier}", otpIdentifier);
                throw new InvalidOperationException("Failed to resend OTP.", ex);
            }
        }

        /// <inheritdoc/>
        public string GenerateOtpToken(ApplicationUser user, string otpIdentifier)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JWTSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret not configured"));

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("OtpIdentifier", otpIdentifier),
                        new Claim("Purpose", "OtpValidation")
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _configuration["JWTSettings:ValidIssuer"],
                    Audience = _configuration["JWTSettings:ValidAudience"]
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP token for user: {Email}", user?.Email);
                throw new InvalidOperationException("Failed to generate OTP token.", ex);
            }
        }

        /// <inheritdoc/>
        public ClaimsPrincipal? ValidateOtpToken(string otpToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JWTSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured"));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWTSettings:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWTSettings:ValidAudience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(otpToken, tokenValidationParameters, out _);

                // Verify token purpose
                var purpose = principal.FindFirst("Purpose")?.Value;
                if (purpose != "OtpValidation")
                    return null;

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OTP token validation failed");
                return null;
            }
        }

        /// <summary>
        /// Generates a random 6-digit OTP
        /// </summary>
        private static string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        /// <summary>
        /// Generates HTML email body for OTP verification
        /// </summary>
        private static string GenerateOtpEmailBody(ApplicationUser user, string otp)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Verification Code</title>
                    <style>
                        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f5; }}
                        .container {{ max-width: 520px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; }}
                        .header h1 {{ color: white; margin: 0; font-size: 24px; font-weight: 600; }}
                        .content {{ padding: 40px 30px; text-align: center; }}
                        .otp-code {{ font-size: 36px; font-weight: 700; letter-spacing: 8px; color: #667eea; background: #f0f0ff; padding: 20px; border-radius: 12px; margin: 20px 0; font-family: monospace; }}
                        .button {{ display: inline-block; padding: 12px 32px; background: linear-gradient(135deg, #667eea, #764ba2); color: white; text-decoration: none; border-radius: 8px; margin: 20px 0; font-weight: 600; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #6c757d; background-color: #f9fafb; border-top: 1px solid #e5e7eb; }}
                        .note {{ background-color: #fef3c7; padding: 12px; border-radius: 8px; margin: 20px 0; font-size: 13px; border-left: 4px solid #f59e0b; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Verification Required</h1>
                        </div>
                        <div class='content'>
                            <p>Hello <strong>{System.Security.SecurityElement.Escape(user.FirstName)} {System.Security.SecurityElement.Escape(user.LastName)}</strong>,</p>
                            <p>Please use the verification code below to complete your login:</p>
                            <div class='otp-code'>{otp}</div>
                            <div class='note'>
                                <strong>⚠️ Security Note:</strong> This code will expire in 5 minutes.
                            </div>
                            <p>If you didn't attempt to login, please ignore this email.</p>
                            <p><strong>Never share this code with anyone, including support staff.</strong></p>
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