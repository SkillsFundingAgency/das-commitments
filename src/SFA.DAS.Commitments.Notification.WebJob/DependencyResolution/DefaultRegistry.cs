﻿using Microsoft.Azure;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data;
using SFA.DAS.Commitments.Notification.WebJob.Configuration;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client.Configuration;
using StructureMap;
using System;
using System.Reflection;

using SFA.DAS.Commitments.Infrastructure.Configuration;
using SFA.DAS.Commitments.Notification.WebJob.Services;

namespace SFA.DAS.Commitments.Notification.WebJob.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                });

            var config = GetConfiguration("SFA.DAS.CommitmentNotification");
            For<CommitmentNotificationConfiguration>().Use(config);
            ConfigureEmailWrapper(config);

            For<IAccountApiClient>().Use<AccountApiClient>()
                .Ctor<IAccountApiConfiguration>().Is(config.AccountApi);
            For<ProviderUserApiConfiguration>().Use(config.ProviderUserApi);

            For<IApprenticeshipRepository>().Use<ApprenticeshipRepository>().Ctor<string>().Is(config.DatabaseConnectionString);

            For<IEmployerEmailTemplatesService>().Use<EmployerEmailTemplatesService>();
            For<IProviderEmailTemplatesService>().Use<ProviderEmailTemplatesService>();
            For<INotificationJob>().Use<NotificationJob>();

            For<INotificationsApiClientConfiguration>().Use(config.NotificationApi);

            For<IConfiguration>().Use(config);
            For<ILog>().Use(x => new NLogLogger(x.ParentType, null, null)).AlwaysUnique();
        }

        private void ConfigureEmailWrapper(CommitmentNotificationConfiguration config)
        {
            if (config.UseIdamsService)
                For<IProviderEmailServiceWrapper>().Use<IdamsEmailServiceWrapper>();
            else
                For<IProviderEmailServiceWrapper>().Use<FakeProviderEmailServiceWrapper>();
        }

        private CommitmentNotificationConfiguration GetConfiguration(string serviceName)
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");

            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository,
                new ConfigurationOptions(serviceName, environment, "1.0"));

            var result = configurationService.Get<CommitmentNotificationConfiguration>();

            return result;
        }

        private static IConfigurationRepository GetConfigurationRepository()
        {
            return new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
        }
    }
}
