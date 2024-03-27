using Microsoft.Extensions.Hosting;
using SFA.DAS.Commitments.Support.SubSite.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class IHostBuilderExtensions
    {
        public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureAppConfiguration(c => c
                .AddAzureTableStorage(
                    CommitmentsSupportConfigurationKeys.CommitmentsSupportSubSite,
                    CommitmentsConfigurationKeys.CommitmentsV2,
                    CommitmentsConfigurationKeys.EncodingConfiguration)
            );
        }
    }
}