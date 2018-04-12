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

        public static CommitmentsApiConfiguration Get()
        {
            var environment = CloudConfigurationManager.GetSetting("EnvironmentName");

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository, new ConfigurationOptions(ServiceName, environment, Version));

            return configurationService.Get<CommitmentsApiConfiguration>();
        }

        private static IConfigurationRepository GetConfigurationRepository()
        {
            return new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
        }
    }
}
