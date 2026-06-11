using NexusCommerce.Domain.Entities.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NexusCommerce.Application.Contracts.Services
{
     /// <summary>
    /// Interface for JWT (JSON Web Token) generation and management
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for the specified user with full claims and roles.
        /// </summary>
        /// <param name="user">The application user for whom the token is generated.</param>
        /// <returns>A JWT token as a string.</returns>
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);

        /// <summary>
        /// Creates a JwtSecurityToken object with claims and roles (used internally or for advanced scenarios).
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        /// <returns>A <see cref="JwtSecurityToken"/> instance.</returns>
        Task<JwtSecurityToken> CreateTokenAsync(ApplicationUser user);

        /// <summary>
        /// Gets the principal from an expired token (used for token refresh).
        /// </summary>
        /// <param name="token">The expired token.</param>
        /// <returns>The claims principal from the expired token.</returns>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
