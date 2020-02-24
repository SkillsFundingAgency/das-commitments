using Microsoft.Azure;
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
using System.Net.Http;
using SFA.DAS.Commitments.Infrastructure.Configuration;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Commitments.Notification.WebJob.EmailServices;
using SFA.DAS.Commitments.Notification.WebJob.Services;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.Notifications.Api.Client;
using IConfiguration = SFA.DAS.Commitments.Domain.Interfaces.IConfiguration;

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

            var config = GetConfiguration(ConfigurationKeys.NotificationsApiClient);
            For<CommitmentNotificationConfiguration>().Use(config);
            ConfigureEmailWrapper(config);

            For<IAccountApiClient>().Use<AccountApiClient>()
                .Ctor<IAccountApiConfiguration>().Is(config.AccountApi);

            For<ProviderUserApiConfiguration>().Use(config.ProviderUserApi);
            For<ICurrentDateTime>().Use(x => new CurrentDateTime());
            For<IApprenticeshipRepository>().Use<ApprenticeshipRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<ICommitmentRepository>().Use<CommitmentRepository>().Ctor<string>().Is(config.DatabaseConnectionString);

            For<IEmployerAlertSummaryEmailService>().Use<EmployerAlertSummaryEmailService>();
            For<IProviderAlertSummaryEmailService>().Use<ProviderAlertSummaryEmailService>();
            For<ISendingEmployerTransferRequestEmailService>().Use<SendingEmployerTransferRequestEmailService>();
            For<INotificationJob>().Use<NotificationJob>();
            For<INotificationsApiClientConfiguration>().Use(c => c.GetInstance<NotificationsApiClientConfiguration>());
            For<PAS.Account.Api.Client.IAccountApiClient>().Use<PAS.Account.Api.Client.AccountApiClient>()
                .Ctor<PAS.Account.Api.Client.IAccountApiConfiguration>().Is(config.ProviderAccountUserApi);

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
