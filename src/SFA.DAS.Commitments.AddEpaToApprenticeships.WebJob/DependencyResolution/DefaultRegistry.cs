using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using SFA.DAS.AssessmentOrgs.Api.Client;
using SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.Configuration;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;
using StructureMap;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.DependencyResolution
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

            var config = GetConfiguration("SFA.DAS.AddEpaToApprenticeships");

            For<IAssessmentOrgsApiClient>().Use<AssessmentOrgsApiClient>()
                .Ctor<string>().Is(config.AssessmentOrgsApiBaseUri);

            For<IPaymentsEventsApiClient>().Use<PaymentsEventsApiClient>()
                .Ctor<IPaymentsEventsApiConfiguration>().Is(config.PaymentEventsApi);

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
            //if (environment.Equals("LOCAL") || environment.Equals("AT") || environment.Equals("TEST"))
            //{
            //    //todo: is this required?
            //    PopulateSystemDetails(environment);
            //}

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository,
                new ConfigurationOptions(serviceName, environment, "1.0"));

            var result = configurationService.Get<AddEpaToApprenticeshipsConfiguration>();

            return result;
        }

        private static IConfigurationRepository GetConfigurationRepository()
        {
            return new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
        }

        private void ConfigurePaymentsApiService(AddEpaToApprenticeshipsConfiguration config)
        {
            if (config.UseDocumentRepository)
            {
                For<IPaymentEvents>().Use<PaymentEventsDocumentSerivce>()
                    .Ctor<string>().Is(config.StorageConnectionString ?? "UseDevelopmentStorage=true");
            }
            else
            {
                For<IPaymentEvents>().Use<PaymentEventsService>();
            }
        }
    }
}
