using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.CommitmentsV2.Api.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureAppConfiguration(c => c
            .AddAzureTableStorage(CommitmentsConfigurationKeys.CommitmentsV2,
                CommitmentsConfigurationKeys.EncodingConfiguration)
        );
    }
}