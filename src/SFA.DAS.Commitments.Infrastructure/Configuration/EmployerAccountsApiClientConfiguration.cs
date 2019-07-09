using SFA.DAS.EAS.Account.Api.Client;

namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    public class EmployerAccountsApiClientConfiguration : IAccountApiConfiguration
    {
        public string ApiBaseUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string IdentifierUri { get; set; }

        public string Tenant { get; set; }
    }
}
