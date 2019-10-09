using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Shared.Configuration;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Shared.DependencyInjection
{
    public class CommitmentsSharedConfigurationRegistry : Registry
    {
        public CommitmentsSharedConfigurationRegistry()
        {
            AddConfiguration<CourseApiClientConfiguration>(ConfigurationKeys.CourseApiConfigKey);
        }

        private void AddConfiguration<T>(string key) where T : class
        {
            For<T>().Use(c => GetConfiguration<T>(c, key)).Singleton();
        }

        private T GetConfiguration<T>(IContext context, string name)
        {
            var configuration = context.GetInstance<IConfiguration>();
            var section = configuration.GetSection(name);
            var value = section.Get<T>();

            return value;
        }
    }
}
