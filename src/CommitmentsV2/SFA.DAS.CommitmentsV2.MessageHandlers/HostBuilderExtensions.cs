using System;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers
{
    public static class HostBuilderExtensions
    {

        public static IHostBuilder ConfigureDasWebJobs(this IHostBuilder builder)
        {
            builder.ConfigureWebJobs(b => b.AddAzureStorageCoreServices().AddTimers());

//pragma warning disable 618
            builder.ConfigureServices(s => s.AddSingleton<IWebHookProvider>(p => null));
//pragma warning restore 618

            return builder;
        }



        public static IHostBuilder MessageHandlerAppConfiguration(this IHostBuilder hostBuilder, string[] args)
        {
            return hostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                var a = context.HostingEnvironment.EnvironmentName;

                builder.AddAzureTableStorage(CommitmentsConfigurationKeys.CommitmentsV2Api)
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
