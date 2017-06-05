using SFA.DAS.Notifications.Api.Client.Configuration;

namespace SFA.DAS.Commitments.Notification.WebJob.Configuration
{
    public class NotificationsApiClientConfiguration : INotificationsApiClientConfiguration
    {
        public string BaseUrl { get; set; }

        public string ClientToken { get; set; }
    }
}
