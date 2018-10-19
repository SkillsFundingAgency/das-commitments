using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Messaging.AzureServiceBus.StructureMap;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Configuration
{
    public class CommitmentsAcademicYearEndProcessorConfiguration : IConfiguration, ITopicMessagePublisherConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string CurrentStartTime { get; set; }
        public string MessageServiceBusConnectionString { get; set; }
    }
}
