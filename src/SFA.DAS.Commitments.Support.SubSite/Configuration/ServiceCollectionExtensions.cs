using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<CommitmentSupportSiteConfiguartion>(configuration.GetSection(CommitmentsSupportConfigurationKeys.CommitmentsSupportSubSite));
            services.Configure<CommitmentsV2Configuration>(configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2));
            services.Configure<EncodingConfig>(configuration.GetSection(CommitmentsConfigurationKeys.EncodingConfiguration));
            return services;
        }
    }
}