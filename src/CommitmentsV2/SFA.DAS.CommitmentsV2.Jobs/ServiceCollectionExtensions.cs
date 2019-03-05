using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Jobs.Configuration;

namespace SFA.DAS.CommitmentsV2.Jobs
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJobConfigurationSections(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            services.AddOptions();
            services.Configure<CommitmentsV2Configuration>(configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2));
            services.Configure<ApprenticeshipInfoServiceApiConfiguration>(configuration.GetSection(CommitmentsConfigurationKeys.ApprenticeshipInfoServiceApi));
            return services;
        }
    }
}