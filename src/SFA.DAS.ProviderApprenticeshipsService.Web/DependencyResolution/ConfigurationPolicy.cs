using System;
using System.Linq;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;
using StructureMap;
using StructureMap.Pipeline;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.DependencyResolution
{
    public class ConfigurationPolicy<T> : ConfiguredInstancePolicy
    {
        private const string ServiceName = "SFA.DAS.ProviderApprenticeshipsService";

        protected override void apply(Type pluginType, IConfiguredInstance instance)
        {

            var serviceConfigurationParamater = instance?.Constructor?.GetParameters().FirstOrDefault(x => x.ParameterType == typeof(T));

            if (serviceConfigurationParamater != null)
            {
                var environment = Environment.GetEnvironmentVariable("DASENV");
                if (string.IsNullOrEmpty(environment))
                {
                    environment = CloudConfigurationManager.GetSetting("EnvironmentName");
                }

                var configurationRepository = GetConfigurationRepository();
                var configurationService = new ConfigurationService(configurationRepository,
                    new ConfigurationOptions(ServiceName, environment, "1.0"));

                var result = configurationService.Get<ProviderApprenticeshipsServiceConfiguration>();
                if (result != null)
                {
                    instance.Dependencies.AddForConstructorParameter(serviceConfigurationParamater, result);
                }
            }
        }

        private IConfigurationRepository GetConfigurationRepository()
        {
            var connectionString = CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString");

            return new AzureTableStorageConfigurationRepository(connectionString);
        }

    }
}