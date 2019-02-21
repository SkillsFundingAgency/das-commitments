using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.CommitmentsV2.Api.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<AzureActiveDirectoryApiConfiguration>(configuration.GetSection("AzureADApiAuthentication"));
            return services;
        }
    }
}
