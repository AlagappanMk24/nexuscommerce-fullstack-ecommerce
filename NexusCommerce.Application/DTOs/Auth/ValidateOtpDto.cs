using System.ComponentModel.DataAnnotations;

namespace NexusCommerce.Application.DTOs.Auth
{
    /// <summary>
    /// Data transfer object for OTP validation
    /// </summary>
    public class ValidateOtpDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The 6-digit OTP code
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "OTP must be 6 digits")]
        public string Otp { get; set; } = string.Empty;

        /// <summary>
        /// Temporary OTP token from login
        /// </summary>
        [Required]
        public string OtpToken { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier for the OTP session
        /// </summary>
        [Required]
        public string OtpIdentifier { get; set; } = string.Empty;
    }
}