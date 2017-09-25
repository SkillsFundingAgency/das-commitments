using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Configuration
{
    public class CommitmentsAcademicYearEndProcessorConfiguration : IConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string CurrentStartTime { get; set; }
    }
}
