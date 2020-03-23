using System;
using System.Configuration;
using SFA.DAS.Configuration;
using SFA.DAS.Encoding;
using StructureMap;

namespace SFA.DAS.Commitments.Api.DependencyResolution
{
    public class EncodingRegistry : Registry
    {
        public const string EncodingConfiguration = "SFA.DAS.Encoding";

        public EncodingRegistry()
        {
            For<EncodingConfig>().Use(ctx => GetConfig(ctx)).Singleton();
            For<IEncodingService>().Use<EncodingService>().Singleton();
        }

        public EncodingConfig GetConfig(IContext ctx)
        {
            var configurationRepository = ctx.GetInstance<IConfigurationRepository>();
            var environmentName = GetEnvironmentName();
            var configurationOptions =
                new ConfigurationOptions(EncodingConfiguration, environmentName, "1.0");

            var svc = new ConfigurationService(configurationRepository, configurationOptions);

            return svc.Get<EncodingConfig>();
        }

        private string GetEnvironmentName()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = ConfigurationManager.AppSettings["EnvironmentName"];
            }

            return environment;
        }
    }
}
