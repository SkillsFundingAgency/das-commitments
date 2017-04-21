using System;
using System.Reflection;
using Microsoft.Azure;
using NLog;
using SFA.DAS.CommitmentPayments.WebJob.Configuration;
using SFA.DAS.CommitmentPayments.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.NLog.Logger;
using StructureMap;

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
            For<IConfiguration>().Use(config);
            For<IDataLockUpdater>().Use<DataLockerUpdater>();
            For<ILog>().Use(x => new NLogLogger(x.ParentType, new DummyRequestContext(), null)).AlwaysUnique();
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

    public class DummyRequestContext : IRequestContext
    {
        public string Url { get; }

        public string IpAddress { get; }
    }

    public static class SystemDetails
    {
        public static string VersionNumber { get; set; }
        public static string EnvironmentName { get; set; }
    }
}
