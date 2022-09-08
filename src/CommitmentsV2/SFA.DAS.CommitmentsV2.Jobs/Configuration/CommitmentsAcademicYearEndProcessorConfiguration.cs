using SFA.DAS.CommitmentsV2.Domain.Interfaces;
//using SFA.DAS.Messaging.AzureServiceBus.StructureMap;

namespace SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob.Configuration
{
    public class CommitmentsAcademicYearEndProcessorConfiguration : IConfig
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string CurrentStartTime { get; set; }
        public string MessageServiceBusConnectionString { get; set; }
    }
}
