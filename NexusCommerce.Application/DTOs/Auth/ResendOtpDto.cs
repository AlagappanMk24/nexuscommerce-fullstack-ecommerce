using System.ComponentModel.DataAnnotations;

namespace NexusCommerce.Application.DTOs.Auth
{
    /// <summary>
    /// Data transfer object for requesting OTP resend
    /// </summary>
    public class ResendOtpDto
    {
        /// <summary>
        /// Unique identifier for the OTP session
        /// </summary>
        [Required]
        public string OtpIdentifier { get; set; } = string.Empty;
    }
}