using System;
using Microsoft.Azure;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Configuration;

namespace SFA.DAS.ProviderCommitments.Services
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
            var environment = Environment.GetEnvironmentVariable(Constants.EnvironmentVariableNames.EnvironmentName);

            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting(Constants.EnvironmentVariableNames.EnvironmentName);
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
                case Constants.EnvironmentNames.Local: return EnvironmentType.Local;
                case Constants.EnvironmentNames.AT: return EnvironmentType.AT;
                case Constants.EnvironmentNames.Test: return EnvironmentType.Test;
                case Constants.EnvironmentNames.Test2: return EnvironmentType.Test;
                case Constants.EnvironmentNames.PreProd: return EnvironmentType.PreProd;
                case Constants.EnvironmentNames.Production: return EnvironmentType.Production;
                case Constants.EnvironmentNames.Demo: return EnvironmentType.Demo;
                case Constants.EnvironmentNames.ModelOffice: return EnvironmentType.ModelOffice;
                default:
                    throw new InvalidEnvironmentException(environmentName);
            }
        }
    }
}