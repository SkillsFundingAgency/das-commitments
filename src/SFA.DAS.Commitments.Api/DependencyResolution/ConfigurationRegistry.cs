using SFA.DAS.Configuration;
using StructureMap;

namespace SFA.DAS.Commitments.Api.DependencyResolution
{
    public class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry()
        {
            //For<IConfigurationService>().Use<ConfigurationService>().Singleton();
            //For<ConfigurationOptions>().Use(Infrastructure.Configuration.Configuration.ConfigurationOptions);
            //For<IConfigurationRepository>().Use(Infrastructure.Configuration.Configuration.ConfigurationRepository);
        }
    }
}