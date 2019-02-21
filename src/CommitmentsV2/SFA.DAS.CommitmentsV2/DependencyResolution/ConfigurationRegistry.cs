using System;
using System.Collections.Generic;
using System.Text;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry()
        {
            //IncludeRegistry<AutoConfigurationRegistry>();
            //For<EmployerFeaturesConfiguration>().Use(c =>c.GetInstance<IAutoConfigurationService>().Get<EmployerFeaturesConfiguration>(ConfigurationKeys.EmployerFeatures)).Singleton();
            //For<GoogleAnalyticsConfiguration>()
            //    .Use(c => c.GetInstance<IGoogleAnalyticsConfigurationFactory>().CreateConfiguration()).Singleton();
            //For<IAzureActiveDirectoryConfiguration>()
            //    .Use(c => c.GetInstance<ProviderRelationshipsConfiguration>().AzureActiveDirectory).Singleton();
            //For<IEmployerUrlsConfiguration>().Use(c => c.GetInstance<ProviderRelationshipsConfiguration>().EmployerUrls)
            //    .Singleton();
            //For<IOidcConfiguration>().Use(c => c.GetInstance<ProviderRelationshipsConfiguration>().Oidc).Singleton();
            //For<ProviderRelationshipsConfiguration>().Use(c =>
            //    c.GetInstance<IAutoConfigurationService>()
            //        .Get<ProviderRelationshipsConfiguration>(ConfigurationKeys.ProviderRelationships)).Singleton();
            //For<ReadStoreConfiguration>().Use(c => c.GetInstance<ProviderRelationshipsConfiguration>().ReadStore)
            //    .Singleton();
            //For<IGoogleAnalyticsConfigurationFactory>().Use<GoogleAnalyticsConfigurationFactory>();
        }
    }
}
