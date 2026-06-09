using Microsoft.Extensions.DependencyInjection;
using NexusCommerce.Application.Contracts.Services;
using NexusCommerce.Application.Services;

namespace NexusCommerce.Application.ServiceExtensions
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            services.AddScoped<IAuthManager, AuthManager>();
            return services;
        }
    }
}