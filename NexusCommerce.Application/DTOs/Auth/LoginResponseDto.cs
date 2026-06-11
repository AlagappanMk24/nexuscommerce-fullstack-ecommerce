namespace NexusCommerce.Application.DTOs.Auth
{
    /// <summary>
    /// Response DTO for login operation
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// User ID for OTP verification
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Email of the user
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Temporary OTP token
        /// </summary>
        public string? OtpToken { get; set; }

        /// <summary>
        /// OTP session identifier
        /// </summary>
        public string? OtpIdentifier { get; set; }

        /// <summary>
        /// Message to display to user
        /// </summary>
        public string? Message { get; set; }
    }
}