using SFA.DAS.Commitments.Application.Configuration;
using SFA.DAS.Commitments.Domain.Configuration;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Messaging.AzureServiceBus.StructureMap;

namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    public class CommitmentsApiConfiguration : IConfiguration, ITopicMessagePublisherConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string Hashstring { get; set; }
        public string AllowedHashstringCharacters { get; set; }
        public EventsApiClientConfiguration EventsApi { get; set; }
        public string MessageServiceBusConnectionString { get; set; }
        public NServiceBusConfiguration NServiceBusConfiguration { get; set; }
        public EmployerAccountsApiClientConfiguration AccountApi { get; set; }
    }
}
