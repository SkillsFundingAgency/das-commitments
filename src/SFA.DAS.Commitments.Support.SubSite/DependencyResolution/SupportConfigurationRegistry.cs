﻿using Microsoft.Extensions.Configuration;
using SFA.DAS.Commitments.Support.SubSite.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class SupportConfigurationRegistry : Registry
    {
        public SupportConfigurationRegistry()
        {
            AddConfiguration<CommitmentSupportSiteConfiguartion>(CommitmentsSupportConfigurationKeys.CommitmentsSupportSubSite);
           
            AddConfiguration<CommitmentsV2Configuration>(CommitmentsConfigurationKeys.CommitmentsV2);
           
            AddConfiguration<EncodingConfig>(CommitmentsConfigurationKeys.EncodingConfiguration);
           
            AddConfiguration<Authorization.Features.Configuration.FeaturesConfiguration>(CommitmentsConfigurationKeys.Features);
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