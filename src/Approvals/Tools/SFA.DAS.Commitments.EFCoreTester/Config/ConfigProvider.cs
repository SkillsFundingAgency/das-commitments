using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;

namespace SFA.DAS.Commitments.EFCoreTester.Config
{
    public class ConfigProvider : IConfigProvider
    {
        private readonly string _configLocation;

        private readonly ConcurrentDictionary<Type, object> _config = new ConcurrentDictionary<Type, object>();
        private readonly Lazy<JObject> _fullConfigFile;
        private const string ConfigFileName = "SFA.DAS.Commitments.EFCoreTester.json";

        public ConfigProvider(string configLocation)
        {
            _configLocation = configLocation;
            _fullConfigFile = new Lazy<JObject>(LoadConfigFile);
        }

        public TConfigType Get<TConfigType>() where TConfigType : class, new()
        {
            return _config.GetOrAdd(typeof(TConfigType), LoadConfig) as TConfigType;
        }

        private object LoadConfig(Type configType)
        {
            return _fullConfigFile.Value.Value<JObject>(configType.Name).ToObject(configType);
        }

        private JObject LoadConfigFile()
        {
            var configFileName = GetConfigFileName();

            return JObject.Parse(File.ReadAllText(configFileName));
        }

        private string GetConfigFileName()
        {
            // Nothing supplied...
            if (string.IsNullOrWhiteSpace(_configLocation))
            {
                return ConfigFileName;
            }

            // Only a directory name...
            if (Directory.Exists(_configLocation))
            {
                return Path.Combine(_configLocation, ConfigFileName);
            }

            // A full file path...
            if (File.Exists(_configLocation))
            {
                return _configLocation;
            }

            throw new FileNotFoundException($"The configuration file {_configLocation} was not found");
        }
    }
}
