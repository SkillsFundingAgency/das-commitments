using Microsoft.AspNetCore.Hosting;
using SFA.DAS.Commitments.Support.SubSite.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Commitments.Support.SubSite.Extentions
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureDasAppConfiguration(this IWebHostBuilder hostBuilder)
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