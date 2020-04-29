﻿using Microsoft.Extensions.Configuration;
using SFA.DAS.Authorization.Features.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class ConfigurationRegistry : Registry
    {
        public ConfigurationRegistry()
        {
            AddConfiguration<CommitmentsV2Configuration>(CommitmentsConfigurationKeys.CommitmentsV2);
            AddConfiguration<AccountApiConfiguration>(CommitmentsConfigurationKeys.AccountApi);
            AddConfiguration<ApprenticeshipInfoServiceConfiguration>(CommitmentsConfigurationKeys.ApprenticeshipInfoService);
            AddConfiguration<AzureActiveDirectoryApiConfiguration>(CommitmentsConfigurationKeys.AzureActiveDirectoryApiConfiguration);
            AddConfiguration<FeaturesConfiguration>(CommitmentsConfigurationKeys.Features);
            AddConfiguration<EncodingConfig>(CommitmentsConfigurationKeys.EncodingConfiguration);
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
