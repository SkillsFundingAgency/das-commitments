using System;
using System.Configuration;
using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Configuration;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.NLog.Logger;
using StructureMap;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.AssembliesAndExecutablesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                });


            var config = GetConfiguration("SFA.DAS.CommitmentsAcademicYearEndProcessor");
            For<ILog>().Use(x => new NLogLogger(x.ParentType, new ConsoleLoggingContext(), null)).AlwaysUnique();

            DateTime? currentDatetime = null;
            if (!string.IsNullOrWhiteSpace(config.CurrentStartTime))
            {
                currentDatetime = DateTime.Parse(config.CurrentStartTime);
            }

            For<ICurrentDateTime>().Use(x => new CurrentDateTime(currentDatetime));

            For<IDataLockRepository>()
                .Use<DataLockRepository>()
                .Ctor<string>().Is(config.DatabaseConnectionString);
            For<IApprenticeshipUpdateRepository>()
                .Use<ApprenticeshipUpdateRepository>()
                .Ctor<string>().Is(config.DatabaseConnectionString);
            For<IApprenticeshipRepository>()
                .Use<ApprenticeshipRepository>()
                .Ctor<string>().Is(config.DatabaseConnectionString);
        }

        private CommitmentsAcademicYearEndProcessorConfiguration GetConfiguration(string serviceName)
        {
            var environment = ConfigurationManager.AppSettings["EnvironmentName"];
            
            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository,
                new ConfigurationOptions(serviceName, environment, "1.0"));

            var result = configurationService.Get<CommitmentsAcademicYearEndProcessorConfiguration>();
            return result;
        }

        private static IConfigurationRepository GetConfigurationRepository()
        {
            return new AzureTableStorageConfigurationRepository(ConfigurationManager.AppSettings["ConfigurationStorageConnectionString"]);
        }
    }
}
