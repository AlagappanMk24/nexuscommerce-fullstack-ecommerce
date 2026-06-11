namespace NexusCommerce.Application.DTOs.Auth
{
    /// <summary>
    /// Result of OTP generation operation
    /// </summary>
    public class OtpGenerationResult
    {
        /// <summary>
        /// Indicates if OTP generation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Temporary OTP token for validation
        /// </summary>
        public string? OtpToken { get; set; }

        /// <summary>
        /// Unique identifier for the OTP session
        /// </summary>
        public string? OtpIdentifier { get; set; }

        /// <summary>
        /// OTP expiry time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Error message if generation failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}