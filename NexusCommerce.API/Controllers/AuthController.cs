using Microsoft.AspNetCore.Mvc;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Application.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

namespace NexusCommerce.API.Controllers
{
    /// <summary>
    /// Handles authentication operations including registration, login, password reset, and email verification
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController(IAuthManager authManager, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAuthManager _authManager = authManager;
        private readonly ILogger<AuthController> _logger = logger;

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="dto">The registration data transfer object containing user details</param>
        /// <returns>Returns success message if registration is successful</returns>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Invalid model state or registration failed</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var errors = await _authManager.RegisterAsync(dto);
                if (errors != null)
                    return BadRequest(errors);

                return Ok(new { Message = "Registration successful. Please login to continue." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", dto?.Email);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Error = "An error occurred during registration. Please try again later." });
            }
        }

        /// <summary>
        /// Authenticates a user and initiates 2FA flow by sending OTP
        /// </summary>
        /// <param name="dto">The login credentials</param>
        /// <returns>Returns OTP token and identifier for 2FA if credentials are valid</returns>
        /// <response code="200">Login successful, 2FA initiated</response>
        /// <response code="400">Invalid model state</response>
        /// <response code="401">Invalid email or password</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authManager.LoginAsync(dto);

                return Ok(new
                {
                    UserId = result.UserId,
                    OtpToken = result.OtpToken,
                    OtpIdentifier = result.OtpIdentifier,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", dto?.Email);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Error = "An error occurred during login. Please try again later." });
            }
        }

        /// <summary>
        /// Validates OTP and completes the login process
        /// </summary>
        /// <param name="dto">The OTP validation data</param>
        /// <returns>Returns JWT token if OTP is valid</returns>
        /// <response code="200">OTP validated successfully, returns JWT token</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Invalid or expired OTP</response>
        [HttpPost("validate-otp")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateOtp([FromBody] ValidateOtpDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var token = await _authManager.ValidateOtpAndCompleteLoginAsync(dto);

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { Error = "Invalid or expired verification code. Please request a new code." });
                }

                return Ok(new { Token = token, ExpiresIn = 3600 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP validation for identifier: {OtpIdentifier}", dto?.OtpIdentifier);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Error = "An error occurred during verification. Please try again later." });
            }
        }

        /// <summary>
        /// Resends a new OTP to the user's email
        /// </summary>
        /// <param name="dto">The OTP resend request containing the OTP identifier</param>
        /// <returns>Returns success message if OTP was resent</returns>
        /// <response code="200">OTP resent successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="404">OTP session not found</response>
        [HttpPost("resend-otp")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authManager.ResendOtpAsync(dto);

                if (!result)
                {
                    return NotFound(new { Error = "OTP session not found or expired. Please login again." });
                }

                return Ok(new { Message = "A new verification code has been sent to your email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP for identifier: {OtpIdentifier}", dto?.OtpIdentifier);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Error = "An error occurred while resending the verification code. Please try again later." });
            }
        }

        /// <summary>
        /// Sends a password reset link to the user's email address
        /// </summary>
        /// <param name="email">The email address of the user requesting password reset</param>
        /// <returns>Always returns success message for security reasons</returns>
        /// <response code="200">Reset link sent (if email exists)</response>
        /// <response code="400">Invalid email format</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody, EmailAddress] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
                {
                    return BadRequest(new { Error = "A valid email address is required." });
                }

                var resetBaseUrl = "http://localhost:4200/auth/reset-password";
                await _authManager.ForgotPasswordAsync(email, resetBaseUrl);

                // Always return success to avoid email enumeration attacks
                return Ok(new { Message = "If this email is registered, a reset link has been sent." });
            }
            catch (Exception)
            {
                // Log exception here using ILogger
                // Still return success to avoid exposing email existence
                return Ok(new { Message = "If this email is registered, a reset link has been sent." });
            }
        }

        /// <summary>
        /// Resets the user's password using a valid reset token
        /// </summary>
        /// <param name="dto">The reset password data containing email, token, and new password</param>
        /// <returns>Returns success message if password reset is successful</returns>
        /// <response code="200">Password reset successful</response>
        /// <response code="400">Invalid model state or reset failed</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authManager.ResetPasswordAsync(dto);

                if (!result)
                {
                    return BadRequest(new { Error = "Invalid or expired token. Please request a new password reset." });
                }

                return Ok(new { Message = "Password reset successful. You can now login with your new password." });
            }
            catch (Exception)
            {
                // Log exception here using ILogger
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Error = "An error occurred while resetting your password. Please try again later." });
            }
        }
    }
}