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
            public Type Type => Types.Length == 1 ? Types[0] : null;
            public Type[] Types { get; set; }
            public object Instance { get; set; }
            public ConfigObjectStatus Status { get; set; }
            public string ErrorMessage { get; set; }
        }

        private class ConfigCache
        {
            private readonly Dictionary<Type, ConfigObject> _availableTypes = new Dictionary<Type, ConfigObject>();
            private readonly List<ConfigObject> _unavailableTypes = new List<ConfigObject>();

            public IEnumerable<ConfigObject> ConfigItems => _availableTypes.Values.Union(_unavailableTypes);

            public bool TryGetAvailableConfigItem(Type configType, out ConfigObject configItem)
            {
                return _availableTypes.TryGetValue(configType, out configItem);
            }

            public bool TryGetUnavailbleType(Type configType, out ConfigObject configItem)
            {
                configItem = _unavailableTypes.FirstOrDefault(ci => ci.Types.Contains(configType));

                if (configItem == null)
                {
                    configItem = _unavailableTypes.FirstOrDefault(ci => 
                        ci.Status == ConfigObjectStatus.TypeNotFound &&
                        configType.FullName.EndsWith(ci.Name, StringComparison.InvariantCultureIgnoreCase));
                }

                return configItem != null;
            }

            public void AddConfigItem(ConfigObject configItem)
            {
                if (configItem.Status == ConfigObjectStatus.Okay)
                {
                    _availableTypes.Add(configItem.Type, configItem);
                }
                else
                {
                    _unavailableTypes.Add(configItem);
                }
            }
        }

        private readonly IConfigurationRepository _configurationRepository;
        private readonly IAssemblyDiscoveryService _assemblyDiscoveryService;
        private readonly IEnvironmentService _environmentService;

        private readonly Lazy<ConfigCache> _lazyConfig;

        private readonly ILog _logger;
        private DateTime _initialisedTime;

        public ProviderCommitmentsConfigurationService(
            IConfigurationRepository configurationRepository, 
            IAssemblyDiscoveryService assemblyDiscoveryService,
            IEnvironmentService environmentService,
            ILog logger)
        {
            _configurationRepository = configurationRepository;
            _assemblyDiscoveryService = assemblyDiscoveryService;
            _environmentService = environmentService;
            _lazyConfig = new Lazy<ConfigCache>(LoadConfigData);
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
            if (TryGet(requiredConfigType, out object config))
            {
                return config;
            }

            throw new MissingConfigItemException(requiredConfigType.FullName, Config.ConfigItems.Select(t => t.Type?.FullName ?? t.Name).ToArray());
        }

        private ConfigCache Config => _lazyConfig.Value;

        private bool TryGet(Type requiredConfigType, out object config)
        {
            config = null;

            if (Config.TryGetAvailableConfigItem(requiredConfigType, out var availableConfig))
            {
                config = availableConfig.Instance;
                return true;
            }

            if (Config.TryGetUnavailbleType(requiredConfigType, out var unavailableConfig))
            {
                throw new ConfigItemUnavailableException(requiredConfigType.FullName, unavailableConfig.Status,
                    unavailableConfig.ErrorMessage, _initialisedTime);
            }

            return false;
        }
        
        private ConfigCache LoadConfigData()
        {
            var fullConfig = GetFullConfig();

            var configItems = new ConfigCache();

            LoadAllConfigObjects(fullConfig, configItems);

            LogResultsOfConfigLoad(configItems);
           
            return configItems;
        }

        private void LoadAllConfigObjects(JObject fullConfig, ConfigCache configItems)
        {
            foreach (var property in fullConfig.Children().OfType<JProperty>())
            {
                var configItem = MapJsonPropertyToConfigObject(property);

                // Note: we do not need to be concerned with handling duplicate property definitions in the source json 
                // because these are handled by NewtonSoft - see DuplicatePropertyNameHandling. Default is to take last 
                // property value.
                configItems.AddConfigItem(configItem);
            }

            _initialisedTime = DateTime.UtcNow;
        }

        private ConfigObject MapJsonPropertyToConfigObject(JProperty property)
        {
            var types = _assemblyDiscoveryService.GetApplicationTypes(property.Name);

            var configItem = new ConfigObject
            {
                Name = property.Name.ToUpperInvariant(),
                Status = ConfigObjectStatus.Undefined,
                Types = types
            };

            switch (types.Length)
            {
                case 0:
                    configItem.Status = ConfigObjectStatus.TypeNotFound;
                    break;

                case 1:
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

        private JObject GetFullConfig()
        {
            var environmentName = _environmentService.EnvironmentName;
            var config = _configurationRepository.Get(Constants.ServiceName, environmentName, "1.0");
            return JObject.Parse(config);
        }

        private void LogResultsOfConfigLoad(ConfigCache config)
        {
            const int reasonableSizeOfLogMessage = 400;
            var logMessage = new StringBuilder(reasonableSizeOfLogMessage);

            logMessage.AppendLine($"These are the results of loading configuration from {Constants.ServiceName}");
            logMessage.AppendLine($"Only the case-insensitively-named config items named here will be recognised when requesting config.");
            logMessage.AppendLine($"Only the items with status {ConfigObjectStatus.Okay} will be available.");

            foreach (var configItem in config.ConfigItems.OrderBy(ci => ci.Name))
            {
                logMessage.AppendLine($"propertyName:{configItem.Name} status:{configItem.Status} typeName:{configItem.Type?.FullName ?? "<not applicable>"} additionalErrorInfo:{configItem.ErrorMessage ?? "<none>"}");
            }

            _logger.Info(logMessage.ToString());
        }
    }
}