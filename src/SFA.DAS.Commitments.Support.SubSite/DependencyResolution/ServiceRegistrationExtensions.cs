using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Commitments.Support.SubSite.Configuration;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.Commitments.Support.SubSite.Validation;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.Commitments.Support.SubSite.Services;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;

namespace SFA.DAS.Commitments.Support.SubSite.DependencyResolution;

public static class ServiceRegistrationExtensions 
{
    public static IServiceCollection AddSupportSiteDefaultServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<IApprenticeshipMapper, ApprenticeshipMapper>();
        services.AddTransient<IApprenticeshipsOrchestrator, ApprenticeshipsOrchestrator>();
        services.AddTransient<IValidator<ApprenticeshipSearchQuery>, ApprenticeshipsSearchQueryValidator>();
        services.AddTransient<ISiteValidatorSettings>(s => s.GetService<SiteValidatorSettings>());
        services.AddTransient<ICommitmentMapper, CommitmentMapper>();
        services.AddTransient<ICommitmentStatusCalculator, CommitmentStatusCalculator>();

        services.AddCurrentDateTimeService(config);
        services.AddTransient<IEncodingService, EncodingService>();
        services.AddTransient<IEmailOptionalService, EmailOptionalService>();
        services.AddTransient<Learners.Validators.IUlnValidator, Learners.Validators.UlnValidator>();

        services.AddTransient<IMapper<Apprenticeship, SupportApprenticeshipDetails>,
                ApprenticeshipToSupportApprenticeshipDetailsMapperMapper>();

        return services;
    }

    public static IServiceCollection AddSupportConfigurationSections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.AddConfigurationFor<CommitmentSupportSiteConfiguartion>(configuration, CommitmentsSupportConfigurationKeys.CommitmentsSupportSubSite);
        services.AddConfigurationFor<CommitmentsV2Configuration>(configuration, CommitmentsConfigurationKeys.CommitmentsV2);
        services.AddConfigurationFor<EncodingConfig>(configuration, CommitmentsConfigurationKeys.EncodingConfiguration);
        services.AddConfigurationFor<EmailOptionalConfiguration>(configuration, CommitmentsConfigurationKeys.EmailOptionalConfiguration);
        services.AddConfigurationFor<Authorization.Features.Configuration.FeaturesConfiguration>(configuration, CommitmentsConfigurationKeys.Features);

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
