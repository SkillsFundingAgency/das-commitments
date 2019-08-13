using SFA.DAS.Configuration;
using StructureMap;

namespace SFA.DAS.Commitments.Api.DependencyResolution
{
    public class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry()
        {
            For<IConfigurationService>().Use(Infrastructure.Configuration.Configuration.ConfigurationService);
            For<IConfigurationRepository>().Use(Infrastructure.Configuration.Configuration.ConfigurationRepository);
        }
    }
}