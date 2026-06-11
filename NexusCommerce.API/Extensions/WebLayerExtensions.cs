using NexusCommerce.Domain.Settings;

namespace NexusCommerce.API.Extensions
{
    public static class WebLayerExtensions
    {
        #region Public API
        public static IServiceCollection AddWebLayer(this IServiceCollection services, IConfiguration configuration)
        {

            #region CORS Configuration

            ConfigureCORS(services);

            #endregion

            #region Application Settings

            // Configure various application settings
            ConfigureEmailSettings(services, configuration);

            #endregion

            return services;
        }

        #endregion

        #region Private Configuration Methods

        /// <summary>
        /// Configures Cross-Origin Resource Sharing (CORS) policy to allow frontend applications.
        /// </summary>
        /// <param name="services">The service collection to add CORS configuration to.</param>
        /// <remarks>
        /// The configured policy allows requests from:
        /// <list type="bullet">
        /// <item><description>Angular development server (http://localhost:4200, https://localhost:4200)</description></item>
        /// <item><description>Additional development URLs (https://localhost:64622, http://localhost:64622)</description></item>
        /// </list>
        /// The policy allows any header, any method, and credentials.
        /// </remarks>
        private static void ConfigureCORS(IServiceCollection services)
        {
            // Add CORS policy to allow requests from the specified origin
            services.AddCors(options =>
            {
                options.AddPolicy("AllowMyOrigin", builder =>
                    builder.WithOrigins(
                            "http://localhost:4200",
                            "https://localhost:4200",
                            "https://localhost:64622",
                            "http://localhost:64622")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });
        }

        /// <summary>
        /// Configures email service settings from application configuration.
        /// </summary>
        /// <param name="services">The service collection to add settings to.</param>
        /// <param name="configuration">The application configuration containing email settings.</param>
        /// <remarks>
        /// Settings include SMTP server configuration, credentials, and email templates.
        /// Settings are bound to <see cref="EmailSettings"/> class for strongly-typed access.
        /// </remarks>
        private static void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        }

        #endregion
    }
}