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

        private static readonly Lazy<IConfigurationService> LazyConfigurationService = new Lazy<IConfigurationService>(GetConfigurationService);

        public static CommitmentsApiConfiguration Get()
        {
            return ConfigurationService.Get<CommitmentsApiConfiguration>();
        }

        public static string EnvironmentName = GetEnvironmentName();

        public static IConfigurationService ConfigurationService => LazyConfigurationService.Value;

        private static IConfigurationService GetConfigurationService()
        {
            var repo = new AzureTableStorageConfigurationRepository();
            var options = new ConfigurationOptions(ServiceName, EnvironmentName, Version);
            return new ConfigurationService(repo, options);
        }

        private static string GetEnvironmentName()
        {
            return CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString");
        }
    }
}
