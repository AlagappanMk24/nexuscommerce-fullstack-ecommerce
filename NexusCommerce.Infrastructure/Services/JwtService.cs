using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Domain.Entities.Identity;
using NexusCommerce.Infrastructure.Configuration.Settings;
using NexusCommerce.Infrastructure.Data.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NexusCommerce.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for JWT token generation, validation, refresh token management,
    /// and token persistence/revocation.
    /// </summary>
    public class JwtService(
        IOptions<JwtSettings> jwtSettings,
        UserManager<ApplicationUser> userManager,
        ILogger<JwtService> logger) : IJwtService
    {
        #region Private Fields

        private readonly JwtSettings _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly ILogger<JwtService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var token = await CreateTokenAsync(user);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc/>
        public async Task<JwtSecurityToken> CreateTokenAsync(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var claims = await BuildUserClaimsAsync(user);
            var signingCredentials = GetSigningCredentials();

            return new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours),
                signingCredentials: signingCredentials);
        }

        /// <inheritdoc/>
        public ClaimsPrincipal? ValidateOtpToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters(validateLifetime: true);

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "OTP token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "OTP token has invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OTP token validation failed");
                return null;
            }
        }

        /// <inheritdoc/>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters(validateLifetime: false);

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get principal from expired token");
                return null;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds the complete list of claims for a user including roles.
        /// </summary>
        private async Task<List<Claim>> BuildUserClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            };

            // Add roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }

        /// <summary>
        /// Gets the signing credentials using the configured secret key.
        /// </summary>
        private SigningCredentials GetSigningCredentials()
        {
            // Add null check and validation
            if (string.IsNullOrEmpty(_jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured. Please check appsettings.json.");
            }
            var secretKeyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            if (secretKeyBytes.Length == 0)
            {
                throw new InvalidOperationException("JWT SecretKey cannot be empty or zero-length.");
            }

            var key = new SymmetricSecurityKey(secretKeyBytes);
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// Gets the token validation parameters.
        /// </summary>
        /// <param name="validateLifetime">Whether to validate token lifetime.</param>
        private TokenValidationParameters GetTokenValidationParameters(bool validateLifetime = true)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.ValidIssuer,
                ValidAudience = _jwtSettings.ValidAudience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes),
                RequireExpirationTime = true,
                RequireSignedTokens = true
            };
        }

        /// <summary>
        /// Gets the current IP address from the HTTP context.
        /// </summary>
        private static string GetCurrentIpAddress()
        {
            // This should be enhanced to get actual IP from HttpContextAccessor
            // For now, return a placeholder
            return "unknown";
        }
        #endregion
    }
}