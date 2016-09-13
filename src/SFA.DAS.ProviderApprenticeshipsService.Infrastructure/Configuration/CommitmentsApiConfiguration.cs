using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces;

namespace SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration
{
    public class CommitmentsApiConfiguration : IConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientSecret { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
    }
}
