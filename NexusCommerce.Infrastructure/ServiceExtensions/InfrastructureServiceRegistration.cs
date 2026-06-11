using Microsoft.Extensions.DependencyInjection;
using NexusCommerce.Application.Contracts.Persistence;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Application.Services;
using NexusCommerce.Infrastructure.Repositories;
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
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddPersistenceServices();
            return services;
        }
        private static void AddPersistenceServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();
        }
    }
}