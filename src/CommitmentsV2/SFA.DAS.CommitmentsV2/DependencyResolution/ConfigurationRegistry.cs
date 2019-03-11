using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry()
        {
            AddConfiguration<CommitmentsV2Configuration>(CommitmentsConfigurationKeys.CommitmentsV2);
            AddConfiguration<ApprenticeshipInfoServiceConfiguration>(CommitmentsConfigurationKeys.ApprenticeshipInfoService);
            AddConfiguration<AzureActiveDirectoryApiConfiguration>(CommitmentsConfigurationKeys.AzureActiveDirectoryApiConfiguration);
        }

        private void AddConfiguration<T>(string name) where T : class
        {
            For<T>().Use(c => GetInstance<T>(c, name)).Singleton();
        }

        private static T GetInstance<T>(IContext context, string name)
        {
            var configuration = context.GetInstance<IConfiguration>();
            var configSection = configuration.GetSection(name);
            var t = configSection.Get<T>();
            return t;
        }
    }
}
