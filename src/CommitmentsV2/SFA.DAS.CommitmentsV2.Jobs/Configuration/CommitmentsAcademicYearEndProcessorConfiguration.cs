using SFA.DAS.CommitmentsV2.Domain.Interfaces;

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
