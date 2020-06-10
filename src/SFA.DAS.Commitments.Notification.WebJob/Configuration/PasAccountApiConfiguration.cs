using SFA.DAS.PAS.Account.Api.Client;

namespace SFA.DAS.Commitments.Notification.WebJob.Configuration
{
    public class PasAccountApiConfiguration : IPasAccountApiConfiguration
    {
        public string ApiBaseUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string IdentifierUri { get; set; }

        public string Tenant { get; set; }
    }
}