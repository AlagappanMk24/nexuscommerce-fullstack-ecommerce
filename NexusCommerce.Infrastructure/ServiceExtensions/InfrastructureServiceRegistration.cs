using Microsoft.Extensions.DependencyInjection;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Infrastructure.Services;

namespace NexusCommerce.Infrastructure.ServiceExtensions
{
    /// <summary>
    /// Centralized registration for Infrastructure layer dependencies.
    /// Organized by functional domain to ensure maintainability.
    /// </summary>
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}