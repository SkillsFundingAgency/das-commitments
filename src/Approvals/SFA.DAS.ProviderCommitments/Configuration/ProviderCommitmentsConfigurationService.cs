using SFA.DAS.Configuration;

namespace SFA.DAS.ProviderCommitments.Configuration
{
    public class ProviderCommitmentsConfigurationService : IProviderCommitmentsConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IEnvironmentService _environmentService;

        public ProviderCommitmentsConfigurationService(IConfigurationRepository configurationRepository, IEnvironmentService environmentService)
        {
            _configurationRepository = configurationRepository;
            _environmentService = environmentService;
        }

        public T GetConfiguration<T>(string serviceName) where T : new()
        {
            var environmentName = _environmentService.EnvironmentName;

            var configurationService = new ConfigurationService(
                _configurationRepository,
                new ConfigurationOptions(serviceName, environmentName, "1.0"));

            var config = configurationService.Get<T>();

            return config;
        }

        public ProviderCommitmentsSecurityConfiguration GetSecurityConfiguration()
        {
            return GetConfiguration<ProviderCommitmentsSecurityConfiguration>("SFA.DAS.ProviderCommitments");
        }
    }
}