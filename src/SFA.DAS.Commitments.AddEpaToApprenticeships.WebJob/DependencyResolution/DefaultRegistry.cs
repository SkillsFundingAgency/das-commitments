using System;
using Microsoft.Azure;
using SFA.DAS.AssessmentOrgs.Api.Client;
using SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.Configuration;
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

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            //AssemblyName an = AssemblyName.GetAssemblyName(@"C:\git\das-commitments\src\SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob\bin\Debug\SFA.DAS.NLog.Logger.dll");
            //var xxx = Assembly.Load(an);
            //var yyy = xxx.GetExportedTypes();

            Scan(
                scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                });

            var config = GetConfiguration("SFA.DAS.Commitments.AddEpaToApprenticeships");

            // ms fake would be preferable
            For<ICurrentDateTime>().Use(x => new CurrentDateTime());

            For<IPaymentsEventsApiClient>().Use<PaymentsEventsApiClient>()
                .Ctor<IPaymentsEventsApiConfiguration>().Is(config.PaymentsEventsApi);

            For<IAssessmentOrgsApiClient>().Use<AssessmentOrgsApiClient>()
                .Ctor<string>().Is(config.AssessmentOrgsApiBaseUri);

            For<IAssessmentOrganisationRepository>().Use<AssessmentOrganisationRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IApprenticeshipRepository>().Use<ApprenticeshipRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IJobProgressRepository>().Use<JobProgressRepository>().Ctor<string>().Is(config.DatabaseConnectionString);

            For<IAzureBlobStorage>().Use<AzureBlobStorage>()
                .Ctor<string>().Is(config.StorageConnectionString ?? "UseDevelopmentStorage=true");

            ConfigurePaymentsApiService(config);
            ConfigureAssessmentOrgsService(config);

            For<IAddEpaToApprenticeships>().Use<AddEpaToApprenticeships>();

            For<ILog>().Use(x => new NLogLogger(x.ParentType, new ConsoleLoggingContext(), null)).AlwaysUnique();
        }

        //todo: config doesn't really belong in DefaultRegistry?
        private AddEpaToApprenticeshipsConfiguration GetConfiguration(string serviceName)
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository,
                new ConfigurationOptions(serviceName, environment, "1.0"));

            return configurationService.Get<AddEpaToApprenticeshipsConfiguration>();
        }

        private static IConfigurationRepository GetConfigurationRepository()
        {
            return new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
        }

        private void ConfigurePaymentsApiService(AddEpaToApprenticeshipsConfiguration config)
        {
            if (config.UsePaymentEventsDocumentRepository)
                For<IPaymentEvents>().Use<PaymentEventsDocumentService>();
            else
                For<IPaymentEvents>().Use<PaymentEventsService>();
        }

        private void ConfigureAssessmentOrgsService(AddEpaToApprenticeshipsConfiguration config)
        {
            if (config.UseAssessmentOrgsDocumentRepository)
                For<IAssessmentOrgs>().Use<AssessmentOrgsDocumentService>();
            else
                For<IAssessmentOrgs>().Use<AssessmentOrgsService>();
        }
    }
}
