using System;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    //todo: better name? not ConfigurationRepository! or CommitmentsApiConfiguration!
    // static, rather than DI, as used by DI setup
    public static class Configuration
    {
        private const string ServiceName = "SFA.DAS.Commitments";
        private const string Version = "1.0";

        private static readonly Lazy<IConfigurationRepository> LazyConfigurationRepository = new Lazy<IConfigurationRepository>(GetConfigurationRepository);

        public static CommitmentsApiConfiguration Get()
        {
            var environment = CloudConfigurationManager.GetSetting("EnvironmentName");

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository, new ConfigurationOptions(ServiceName, environment, Version));

            return configurationService.Get<CommitmentsApiConfiguration>();
        }

        public static string EnvironmentName = GetEnvironmentName();
        public static IConfigurationRepository ConfigurationRepository => LazyConfigurationRepository.Value;
        public static ConfigurationOptions ConfigurationOptions = GetOptions();

        private static IConfigurationRepository GetConfigurationRepository()
        {
            return new AzureTableStorageConfigurationRepository();
        }

        private static ConfigurationOptions GetOptions()
        {
            return new ConfigurationOptions(ServiceName, EnvironmentName, Version);
        }

        private static string GetEnvironmentName()
        {
            return CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString");
        }
    }
}
