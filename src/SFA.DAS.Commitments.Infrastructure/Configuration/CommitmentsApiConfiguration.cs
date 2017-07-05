using SFA.DAS.Commitments.Domain.Configuration;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    public class CommitmentsApiConfiguration : IConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string Hashstring { get; set; }
        public EventsApiClientConfiguration EventsApi { get; set; }
    }
}
