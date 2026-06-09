using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusCommerce.Domain.Entities.Identity;
using NexusCommerce.Infrastructure.Data.Context;
using NexusCommerce.Infrastructure.Data.Seeding;
using NexusCommerce.Infrastructure.Data.Seeding.Abstract;

namespace NexusCommerce.API.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring database services and initialization.
    /// </summary>
    /// <remarks>
    /// This class handles Entity Framework Core configuration, database connection setup,
    /// and database seeding operations for the application.
    /// </remarks>
    public static class DatabaseExtensions
    {
        #region Constants

        private const string DefaultConnectionStringName = "NexusCommerceDbConnection";
        private const int DatabaseSeedTimeoutMinutes = 10;

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers the database context and related services with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration containing connection strings.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>
        /// This method performs the following:
        /// <list type="number">
        /// <item><description>Retrieves the connection string from configuration</description></item>
        /// <item><description>Registers <see cref="CoreInvoiceDbContext"/> with SQL Server as the database provider</description></item>
        /// <item><description>Registers <see cref="IDbInitializer"/> for database seeding operations</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the connection string "CoreAPIDbConnection" is not found in configuration.
        /// </exception>
        /// <example>
        /// <code>
        /// var builder = WebApplication.CreateBuilder(args);
        /// builder.Services.AddDatabase(builder.Configuration);
        /// </code>
        /// </example>
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            #region Connection String Retrieval

            // Retrieve the connection string from appsettings.json or environment variables
            var connectionString = configuration.GetConnectionString(DefaultConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{DefaultConnectionStringName}' not found in configuration.");

            #endregion

            #region DbContext Registration

            // Register Entity Framework Core DbContext with SQL Server
            services.AddDbContext<NexusCommerceContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Enable retry on failure for transient faults (network issues, etc.)
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    sqlOptions.MigrationsAssembly(typeof(NexusCommerceContext).Assembly.FullName);  
                }));

            #endregion

            #region Register ASP.NET Core Identity Services

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                // Configure passwords, lockout settings here if needed
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<NexusCommerceContext>()
            .AddDefaultTokenProviders();

            #endregion

            #region Database Initializer Registration

            // Register database initializer for seeding and migrations
            services.AddScoped<IDbInitializer, DbInitializer>();

            #endregion

            return services;
        }

        /// <summary>
        /// Seeds the database with initial data required for application to function.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> instance.</param>
        /// <returns>A task representing the asynchronous seeding operation.</returns>
        /// <remarks>
        /// This method performs the following:
        /// <list type="number">
        /// <item><description>Creates a service scope to resolve scoped dependencies</description></item>
        /// <item><description>Resolves the <see cref="IDbInitializer"/> and logger services</description></item>
        /// <item><description>Executes database initialization with a configurable timeout</description></item>
        /// <item><description>Logs success or error information for monitoring</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown when database seeding exceeds the configured timeout period.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when an unexpected error occurs during database seeding.
        /// </exception>
        /// <example>
        /// <code>
        /// var app = builder.Build();
        /// await app.SeedDatabaseAsync();
        /// </code>
        /// </example>
        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            #region Service Resolution

            // Create a new service scope to resolve scoped services
            using var scope = app.Services.CreateScope();
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            #endregion

            #region Database Seeding with Timeout

            try
            {
                // Create cancellation token with timeout to prevent indefinite seeding
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(DatabaseSeedTimeoutMinutes));

                logger.LogInformation("Starting database seeding process...");
                await dbInitializer.Initialize(cts.Token);
                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "Database seeding was canceled due to timeout after {TimeoutMinutes} minutes: {Message}",
                 DatabaseSeedTimeoutMinutes, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during database seeding: {Message}", ex.Message);
                throw;
            }
            #endregion
        }
        #endregion
    }
}