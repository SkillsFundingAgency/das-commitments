using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Jobs
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJobConfigurationSections(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            services.AddOptions();
            services.Configure<CommitmentsV2Configuration>(configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2MessageHandler));
            return services;
        }
    }
}