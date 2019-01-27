using Newtonsoft.Json.Linq;
using SFA.DAS.Configuration;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Configuration;
using SFA.DAS.ProviderCommitments.Exceptions;
using SFA.DAS.ProviderCommitments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFA.DAS.ProviderCommitments.Services
{
    public class ProviderCommitmentsConfigurationService : IProviderCommitmentsConfigurationService
    {

        private class ConfigObject
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public object Instance { get; set; }
            public ConfigObjectStatus Status { get; set; }
            public string ErrorMessage { get; set; }
        }

        private readonly IConfigurationRepository _configurationRepository;
        private readonly IEnvironmentService _environmentService;
        private readonly IAssemblyDiscoveryService _assemblyDiscoveryService;
        private readonly Lazy<Dictionary<string, ConfigObject>> _lazyConfig;
        private readonly ILog _logger;
        private DateTime _initialisedTime;

        public ProviderCommitmentsConfigurationService(
            IConfigurationRepository configurationRepository, 
            IEnvironmentService environmentService,
            IAssemblyDiscoveryService assemblyDiscoveryService,
            ILog logger)
        {
            _configurationRepository = configurationRepository;
            _environmentService = environmentService;
            _assemblyDiscoveryService = assemblyDiscoveryService;
            _lazyConfig = new Lazy<Dictionary<string, ConfigObject>>(LoadConfigData);
            _logger = logger;
        }

        public ProviderCommitmentsSecurityConfiguration GetSecurityConfiguration()
        {
            return Get<ProviderCommitmentsSecurityConfiguration>();
        }

        public TConfigType Get<TConfigType>() where TConfigType : class, new()
        {
            var configAsObject = Get(typeof(TConfigType));

            if (configAsObject is TConfigType config)
            {
                return config;
            }

            throw new ConfigItemException($"The config type {typeof(TConfigType).Name} was found but the stored instance is not of the expected type - type of stored instance is {configAsObject.GetType().FullName}");
        }

        public object Get(Type requiredConfigType)
        {
            var requiredConfigName = requiredConfigType.Name.ToUpperInvariant();

            if (TryGet(requiredConfigName, out object config))
            {
                return config;
            }

            throw new MissingConfigItemException(requiredConfigName, _lazyConfig.Value.Keys.ToArray());

        }

        private bool TryGet(string requiredConfigName, out object config)
        {
            config = null;

            if (_lazyConfig.Value.TryGetValue(requiredConfigName, out var configItem))
            {
                if (configItem.Status == ConfigObjectStatus.Okay)
                {
                    config = configItem.Instance;
                    return true;
                }

                throw new ConfigItemUnavailableException(requiredConfigName, configItem.Status, configItem.ErrorMessage,  _initialisedTime);
            }

            return false;
        }
        
        private Dictionary<string, ConfigObject> LoadConfigData()
        {
            var fullConfig = GetFullConfig();

            var configItems = new Dictionary<string, ConfigObject>();

            LoadAllConfigObjects(fullConfig, configItems);

            LogResultsOfConfigLoad(configItems);
           
            return configItems;
        }

        private void LoadAllConfigObjects(JObject fullConfig, Dictionary<string, ConfigObject> configItems)
        {
            foreach (var property in fullConfig.Children().OfType<JProperty>())
            {
                var configItem = MapJsonPropertyToConfigObject(property);

                AddConfigObject(configItems, configItem);
            }

            _initialisedTime = DateTime.UtcNow;
        }

        private ConfigObject MapJsonPropertyToConfigObject(JProperty property)
        {
            var configItem = new ConfigObject
            {
                Name = property.Name.ToUpperInvariant(),
                Status = ConfigObjectStatus.Undefined
            };

            var types = _assemblyDiscoveryService.GetApplicationTypes(property.Name);

            switch (types.Length)
            {
                case 0:
                    configItem.Status = ConfigObjectStatus.TypeNotFound;
                    break;

                case 1:
                    configItem.Type = types[0];
                    try
                    {
                        configItem.Instance = property.Value.ToObject(configItem.Type);
                        configItem.Status = ConfigObjectStatus.Okay;
                    }
                    catch (Exception ex)
                    {
                        configItem.Status = ConfigObjectStatus.CouldNotBeDeserialised;
                        configItem.ErrorMessage = $"{ex.GetType()}:{ex.Message}";
                    }
                    break;

                default:
                    configItem.Status = ConfigObjectStatus.AmbiguousType;
                    break;
            }

            return configItem;
        }

        private static void AddConfigObject(Dictionary<string, ConfigObject> configItems, ConfigObject configItem)
        {
            // Note: we do not need to be concerned with handling duplicate property definitions in the source json 
            // because these are handled by NewtonSoft - see DuplicatePropertyNameHandling. Default is to take last 
            // property value.
            configItems.Add(configItem.Name, configItem);
        }

        private JObject GetFullConfig()
        {
#if DEBUG
            var environmentName = "LOCAL";
#else
            var environmentName = _environmentService.EnvironmentName;
#endif
            var config = _configurationRepository.Get(Constants.ServiceName, environmentName, "1.0");
            return JObject.Parse(config);
        }

        private void LogResultsOfConfigLoad(Dictionary<string, ConfigObject> config)
        {
            const int reasonableSizeOfLogMessage = 400;
            var logMessage = new StringBuilder(reasonableSizeOfLogMessage);

            logMessage.AppendLine($"These are the results of loading configuration from {Constants.ServiceName}");
            logMessage.AppendLine($"Only the case-insensitively-named config items named here will be recognised when requesting config.");
            logMessage.AppendLine($"Only the items with status {ConfigObjectStatus.Okay} will be available.");

            foreach (var configItem in config.OrderBy(kvp => kvp.Key))
            {
                logMessage.AppendLine($"propertyName:{configItem.Key} status:{configItem.Value.Status} typeName:{configItem.Value.Type?.FullName ?? "<not applicable>"} additionalErrorInfo:{configItem.Value.ErrorMessage ?? "<none>"}");
            }

            _logger.Info(logMessage.ToString());
        }
    }
}