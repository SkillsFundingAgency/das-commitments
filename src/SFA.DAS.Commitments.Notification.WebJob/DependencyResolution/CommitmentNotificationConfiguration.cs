using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;

namespace SFA.DAS.Commitments.Notification.WebJob.DependencyResolution
{
    public class CommitmentNotificationConfiguration : IConfiguration
    {
        public string DatabaseConnectionString { get; set; }

        public string ServiceBusConnectionString { get; set; }

        public AccountApiConfiguration AccountApiConfiguration { get; set; }

        public NotificationsApiClientConfiguration NotificationApi { get; set; }

    }

    public class NotificationsApiClientConfiguration : INotificationsApiClientConfiguration
    {
        public string BaseUrl { get; set; }

        public string ClientToken { get; set; }
    }

    public class AccountApiConfiguration : IAccountApiConfiguration
    {
        public string ApiBaseUrl { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string IdentifierUri { get; }

        public string Tenant { get; }
    }
}
