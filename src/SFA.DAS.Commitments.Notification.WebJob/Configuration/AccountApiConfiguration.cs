using SFA.DAS.EAS.Account.Api.Client;

namespace SFA.DAS.Commitments.Notification.WebJob.Configuration
{
    public class AccountApiConfiguration : IAccountApiConfiguration
    {
        public string ApiBaseUrl { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string IdentifierUri { get; }

        public string Tenant { get; }
    }
}
