using System;
using System.Configuration;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    //todo: better name? not ConfigurationRepository! or CommitmentsApiConfiguration!
    // static, rather than DI, as used by DI setup
    public static class Configuration
    {
        private const string ServiceName = "SFA.DAS.Commitments";
        private const string Version = "1.0";
        private static readonly ILog Log = new NLogLogger(typeof(Configuration));

        private static readonly Lazy<ConfigurationServices> LazyConfigurationServices = new Lazy<ConfigurationServices>(GetConfigurationService);

        private class ConfigurationServices
        {
            public IConfigurationService Service { get; set; }
            public IConfigurationRepository Repository { get; set; }
            public ConfigurationOptions Options { get; set; }
        }

        public static CommitmentsApiConfiguration Get()
        {
            return ConfigurationService.Get<CommitmentsApiConfiguration>();
        }

        public static string EnvironmentName = GetEnvironmentName();
        public static IConfigurationService ConfigurationService => LazyConfigurationServices.Value.Service;
        public static IConfigurationRepository ConfigurationRepository => LazyConfigurationServices.Value.Repository;

        private static ConfigurationServices GetConfigurationService()
        {
            try
            {
                Log.Info($"Initialising config for environment {EnvironmentName}");

                var repo = new AzureTableStorageConfigurationRepository(
                    ConfigurationManager.AppSettings["ConfigurationStorageConnectionString"]);
                var options = new ConfigurationOptions(ServiceName, EnvironmentName, Version);
                var service = new ConfigurationService(repo, options);

                var config = new ConfigurationServices
                {
                    Repository = repo,
                    Service = service,
                    Options = options
                };

                Log.Info($"Initialised config for environment {EnvironmentName}");

                return config;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Could not initialise configuration");
                throw;
            }
        }

        private static string GetEnvironmentName()
        {
            return ConfigurationManager.AppSettings["EnvironmentName"];
        }
    }
}
