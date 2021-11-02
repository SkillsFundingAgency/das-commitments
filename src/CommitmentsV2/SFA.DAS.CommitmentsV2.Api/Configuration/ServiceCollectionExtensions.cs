using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Api.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<AzureActiveDirectoryApiConfiguration>(configuration.GetSection($"{CommitmentsConfigurationKeys.CommitmentsV2}:AzureADApiAuthentication"));
            services.Configure<CommitmentsV2Configuration>(configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2));
            services.Configure<EncodingConfig>(configuration.GetSection(CommitmentsConfigurationKeys.EncodingConfiguration));
            services.Configure<LevyTransferMatchingApiConfiguration>(configuration.GetSection(CommitmentsConfigurationKeys.LevyTransferMatchingApiConfiguration));
            return services;
        }

        public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();

            var ltmConfig = configuration.GetSection(CommitmentsConfigurationKeys.LevyTransferMatchingApiConfiguration).Get<LevyTransferMatchingApiConfiguration>();

            services.AddHttpClient<ILevyTransferMatchingApiClient, LevyTransferMatchingClient>(
                config =>
                {
                    config.BaseAddress = new Uri(ltmConfig.BaseUrl);
                    config.DefaultRequestHeaders.Add("X-Version", "1.0");
                });

            return services;
        }
    }
}
