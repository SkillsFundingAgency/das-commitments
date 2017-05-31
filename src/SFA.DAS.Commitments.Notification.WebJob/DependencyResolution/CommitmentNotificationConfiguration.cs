using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Notification.WebJob.DependencyResolution
{
    public class CommitmentNotificationConfiguration : IConfiguration
    {
        public string DatabaseConnectionString { get; set; }

        public string ServiceBusConnectionString { get; set; }
    }
}
