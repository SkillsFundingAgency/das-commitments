using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Authorization.Features.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using SFA.DAS.PAS.Account.Api.ClientV2.Configuration;
using SFA.DAS.ProviderRelationships.Api.Client.Configuration;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.CommitmentsV2.DependencyResolution;

public static class ServiceCollectionConfigurationExtensions
{
    public static IServiceCollection AddConfigurationSections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.AddConfigurationFor<CommitmentsV2Configuration>(configuration, CommitmentsConfigurationKeys.CommitmentsV2);
        services.AddConfigurationFor<AccountApiConfiguration>(configuration, CommitmentsConfigurationKeys.AccountApi);
        services.AddConfigurationFor<ProviderRelationshipsApiConfiguration>(configuration, CommitmentsConfigurationKeys.ProviderRelationshipsApi);
        services.AddConfigurationFor<AzureActiveDirectoryApiConfiguration>(configuration, CommitmentsConfigurationKeys.AzureActiveDirectoryApiConfiguration);
        services.AddConfigurationFor<EncodingConfig>(configuration, CommitmentsConfigurationKeys.EncodingConfiguration);
        services.AddConfigurationFor<ApprovalsOuterApiConfiguration>(configuration, CommitmentsConfigurationKeys.ApprovalsOuterApiConfiguration);
        services.AddConfigurationFor<EmailOptionalConfiguration>(configuration, CommitmentsConfigurationKeys.EmailOptionalConfiguration);
        services.AddConfigurationFor<LevyTransferMatchingApiConfiguration>(configuration, CommitmentsConfigurationKeys.LevyTransferMatchingApiConfiguration);
        services.AddConfigurationFor<FeaturesConfiguration>(configuration, CommitmentsConfigurationKeys.Features);
        services.AddConfigurationFor<PasAccountApiConfiguration>(configuration, CommitmentsConfigurationKeys.ProviderAccountApiConfiguration);
        services.AddConfigurationFor<ReservationsClientApiConfiguration>(configuration, CommitmentsConfigurationKeys.ReservationsClientApiConfiguration);
        services.AddConfigurationFor<RplSettingsConfiguration>(configuration, CommitmentsConfigurationKeys.RplSettingsConfiguration);
        services.AddConfigurationFor<ProviderUrlConfiguration>(configuration, CommitmentsConfigurationKeys.ProviderUrlConfiguration);
        services.AddConfigurationFor<CommitmentPaymentsWebJobConfiguration>(configuration, CommitmentsConfigurationKeys.CommitmentPaymentsWebJobConfiguration);

        return services;
    }

    private static void AddConfigurationFor<T>(this IServiceCollection services, IConfiguration configuration,
        string key) where T : class => services.AddSingleton(GetConfigurationFor<T>(configuration, key));

    private static T GetConfigurationFor<T>(IConfiguration configuration, string name)
    {
        var section = configuration.GetSection(name);
        return section.Get<T>();
    }
}