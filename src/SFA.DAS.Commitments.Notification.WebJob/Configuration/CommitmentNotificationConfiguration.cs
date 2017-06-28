using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Notification.WebJob.Configuration
{
    public class CommitmentNotificationConfiguration : IConfiguration
    {
        public bool EnableJob { get; set; }

        public bool UseIdamsService { get; set; }

        public string TestUserEmail { get; set; }

        public string DatabaseConnectionString { get; set; }

        public string ServiceBusConnectionString { get; set; }

        public AccountApiConfiguration AccountApi { get; set; }

        public NotificationsApiClientConfiguration NotificationApi { get; set; }

    }
}
