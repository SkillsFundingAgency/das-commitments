using System;
using Microsoft.Azure;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.ProviderCommitments.Configuration
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly ILog _logger;

        private readonly Lazy<EnvironmentDetails> _lazyEnvironmentDetails;

        private class EnvironmentDetails
        {
            public string Name { get; set; }
            public EnvironmentType EnvironmentType { get; set; }
        }

        public EnvironmentService(ILog logger)
        {
            _logger = logger;
            _lazyEnvironmentDetails = new Lazy<EnvironmentDetails>(GetEnvironmentDetails);
        }

        public EnvironmentType EnvironmentType => _lazyEnvironmentDetails.Value.EnvironmentType;
        public string EnvironmentName => _lazyEnvironmentDetails.Value.Name;

        private EnvironmentDetails GetEnvironmentDetails()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            if (string.IsNullOrWhiteSpace(environment))
            {
                throw new InvalidEnvironmentException("<null or empty>");
            }

            environment = environment.ToUpperInvariant();
            var environmentType = MapEnvironmentName(environment);

            _logger.Info($"Current process is running in the environment {environment} of type {environmentType}");

            return new EnvironmentDetails
            {
                Name = environment.ToUpperInvariant(),
                EnvironmentType = environmentType
            };
        }

        private EnvironmentType MapEnvironmentName(string environmentName)
        {
            switch(environmentName)
            {
                case "LOCAL": return EnvironmentType.Local;
                case "AT": return EnvironmentType.AT;
                case "TEST": return EnvironmentType.Test;
                case "TEST2": return EnvironmentType.Test;
                case "PP": return EnvironmentType.PreProd;
                case "PRD": return EnvironmentType.Production;
                case "DEMO": return EnvironmentType.Demo;
                case "MO": return EnvironmentType.ModelOffice;
                default:
                    throw new InvalidEnvironmentException(environmentName);
            }
        }
    }
}