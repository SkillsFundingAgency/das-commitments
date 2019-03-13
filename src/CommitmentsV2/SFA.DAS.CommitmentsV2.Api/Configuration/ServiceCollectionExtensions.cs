using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Api.Configuration
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<AzureActiveDirectoryApiConfiguration>(configuration.GetSection($"{CommitmentsConfigurationKeys.CommitmentsV2Base}:AzureADApiAuthentication"));
            services.Configure<CommitmentsV2Configuration>(configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2Base));

            return services;
        }
    }
}
