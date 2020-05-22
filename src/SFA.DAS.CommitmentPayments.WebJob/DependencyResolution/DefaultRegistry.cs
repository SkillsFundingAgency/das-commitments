using System;
using System.Reflection;
using Microsoft.Azure;
using SFA.DAS.CommitmentPayments.WebJob.Configuration;
using SFA.DAS.CommitmentPayments.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.AzureStorage;
using SFA.DAS.Commitments.Infrastructure.Data;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Client.Configuration;
using StructureMap;
using IConfiguration = SFA.DAS.Commitments.Domain.Interfaces.IConfiguration;

namespace SFA.DAS.CommitmentPayments.WebJob.DependencyResolution
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

            var config = GetConfiguration("SFA.DAS.CommitmentPayments");

            For<IPaymentsEventsApiClient>().Use<PaymentsEventsApiClient>()
                .Ctor<IPaymentsEventsApiConfiguration>().Is(config.PaymentEventsApi);

            For<IConfiguration>().Use(config);
            For<CommitmentPaymentsConfiguration>().Use(config);
            For<ICurrentDateTime>().Use(x => new CurrentDateTime());
            For<IDataLockRepository>().Use<DataLockRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IApprenticeshipUpdateRepository>().Use<ApprenticeshipUpdateRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IApprenticeshipRepository>().Use<ApprenticeshipRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IFilterOutAcademicYearRollOverDataLocks>().Use<FilterOutAcademicYearRollOverDataLocks>();

            For<IDataLockUpdater>().Use<DataLockUpdater>();

            For<ILog>().Use(x => new NLogLogger(x.ParentType, new ConsoleLoggingContext(), null)).AlwaysUnique();
            ConfigurePaymentsApiService(config);
        }

        private void ConfigurePaymentsApiService(CommitmentPaymentsConfiguration config)
        {
            if (config.UseDocumentRepository)
            {
                For<IPaymentEvents>().Use<PaymentEventsDocumentService>();

                For<IAzureBlobStorage>().Use<AzureBlobStorage>()
                    .Ctor<string>().Is(config.StorageConnectionString ?? "UseDevelopmentStorage=true");
            }
            else
            {
                For<IPaymentEvents>().Use<PaymentEventsService>();
            }
        }

        private CommitmentPaymentsConfiguration GetConfiguration(string serviceName)
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }
            if (environment.Equals("LOCAL") || environment.Equals("AT") || environment.Equals("TEST"))
            {
                //todo: is this required?
                PopulateSystemDetails(environment);
            }

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository,
                new ConfigurationOptions(serviceName, environment, "1.0"));

            var result = configurationService.Get<CommitmentPaymentsConfiguration>();

            return result;
        }

        private void PopulateSystemDetails(string envName)
        {
            SystemDetails.EnvironmentName = envName;
            SystemDetails.VersionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }


        private static IConfigurationRepository GetConfigurationRepository()
        {
            return new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
        }
    }

    public static class SystemDetails
    {
        public static string VersionNumber { get; set; }
        public static string EnvironmentName { get; set; }
    }
}
