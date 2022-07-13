using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Startup
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder hostBuilder, string[] args)
        {
            return hostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddAzureTableStorage(CommitmentsConfigurationKeys.CommitmentsV2)
                    .AddAzureTableStorage(CommitmentsConfigurationKeys.EncodingConfiguration)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
            });
        }

        public static IHostBuilder UseDasEnvironment(this IHostBuilder hostBuilder)
        {
            var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName);
            var mappedEnvironmentName = DasEnvironmentName.Map[environmentName];

            return hostBuilder.UseEnvironment(mappedEnvironmentName);
        }

        public static IHostBuilder UseStructureMap(this IHostBuilder builder)
        {
            return builder.UseServiceProviderFactory(new StructureMapServiceProviderFactory(null));
        }
    }
}
