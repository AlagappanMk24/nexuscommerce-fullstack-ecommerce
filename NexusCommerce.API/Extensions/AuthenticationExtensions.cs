using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexusCommerce.API.Helpers;
using NexusCommerce.Infrastructure.Configuration.Settings;
using System.Text;

namespace NexusCommerce.API.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring authentication and authorization services.
    /// Implements industry-standard security patterns including JWT bearer authentication,
    /// Identity framework, external OAuth providers, and comprehensive authorization policies.
    /// </summary>
    public static class AuthenticationExtensions
    {
        #region Public Methods

        /// <summary>
        /// Configures Identity framework and JWT bearer authentication with production-ready settings.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The same service collection for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when JWT settings are invalid.</exception>
        public static IServiceCollection AddIdentityAndAuthentication(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            // Configure JWT settings with validation
            var jwtSettings = ConfigureJwtSettings(services, configuration);

            // Configure JWT bearer authentication
            ConfigureJwtAuthentication(services, jwtSettings);

            return services;
        }
        #endregion

        /// <summary>
        /// Configures JWT settings with validation and secret key generation.
        /// </summary>
        private static JwtSettings ConfigureJwtSettings(
            IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind and validate JWT settings
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JWT settings are missing from configuration");

            // Validate settings
            ValidateJwtSettings(jwtSettings);

            // Ensure secret key exists (generate if missing)
            SecretKeyGenerator.EnsureSecretKeyExists(configuration, jwtSettings);

            // Register settings for DI
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

            return jwtSettings;
        }

        /// <summary>
        /// Validates JWT settings for security best practices.
        /// </summary>
        private static void ValidateJwtSettings(JwtSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.SecretKey))
                throw new InvalidOperationException("JWT SecretKey is required");

            if (settings.SecretKey.Length < 32)
                throw new InvalidOperationException("JWT SecretKey must be at least 32 characters");

            if (string.IsNullOrWhiteSpace(settings.ValidIssuer))
                throw new InvalidOperationException("JWT ValidIssuer is required");

            if (string.IsNullOrWhiteSpace(settings.ValidAudience))
                throw new InvalidOperationException("JWT ValidAudience is required");

            if (settings.ExpireHours is < 1 or > 72)
                throw new InvalidOperationException("JWT ExpireHours must be between 1 and 72");

            if (settings.OtpExpireMinutes is < 1 or > 30)
                throw new InvalidOperationException("JWT OtpExpireMinutes must be between 1 and 30");
        }

        /// <summary>
        /// Configures JWT bearer authentication with comprehensive validation.
        /// </summary>
        private static void ConfigureJwtAuthentication(
            IServiceCollection services,
            JwtSettings jwtSettings)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Basic configuration
                options.SaveToken = true;
                options.RequireHttpsMetadata = !IsDevelopment(); // Force HTTPS in production

                // Token validation parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true
                };

                // Custom token validation events
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };

            });
        }

        /// <summary>
        /// Determines if the environment is development.
        /// </summary>
        private static bool IsDevelopment()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        }
    }
}