using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Configuration;

namespace SFA.DAS.CommitmentsV2.Api.Authentication
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddOptions();
            services.Configure<AzureActiveDirectoryConfiguration>(Configuration.GetSection("AzureAd"));
            return services;
        }

    }
}
