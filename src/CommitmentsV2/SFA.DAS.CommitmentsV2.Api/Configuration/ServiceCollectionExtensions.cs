using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Api.Configuration;

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

    public static IServiceCollection AddApiClients(this IServiceCollection services)
    {
        services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
        services.AddHttpClient<ILevyTransferMatchingApiClient, LevyTransferMatchingClient>();
        
        return services;
    }
}