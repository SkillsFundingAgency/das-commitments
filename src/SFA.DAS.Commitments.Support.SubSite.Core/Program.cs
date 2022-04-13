using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Commitments.Support.SubSite.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configBuilder =>
            {
                var config = configBuilder.Build();
                var assemblyName = "SFA.DAS.Support.Commitments";
                configBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = new[] { assemblyName, CommitmentsConfigurationKeys.EncodingConfiguration };
                    options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = config["EnvironmentName"];
                    options.PreFixConfigurationKeys = true;
                });
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
