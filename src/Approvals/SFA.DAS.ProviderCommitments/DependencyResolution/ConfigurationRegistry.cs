using System.Configuration;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.ProviderCommitments.Configuration;
using SFA.DAS.ProviderCommitments.Interfaces;
using SFA.DAS.ProviderCommitments.Services;
using StructureMap;

namespace SFA.DAS.ProviderCommitments.DependencyResolution
{
    public class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry()
        {
            IConfigurationRepository configurationRepository;

            if (bool.Parse(ConfigurationManager.AppSettings["LocalConfig"]))
            {
                configurationRepository = new FileStorageConfigurationRepository();
            }
            else
            {
                configurationRepository =
                    new AzureTableStorageConfigurationRepository(
                        CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            }

            For<IConfigurationRepository>().Use(configurationRepository).Singleton();
            For<IEnvironmentService>().Use<EnvironmentService>().Singleton();
            For<IProviderCommitmentsConfigurationService>().Use<ProviderCommitmentsConfigurationService>().Singleton();
            For<IAssemblyDiscoveryService>().Use<AssemblyDiscoveryService>().Singleton();
        }
    }
}