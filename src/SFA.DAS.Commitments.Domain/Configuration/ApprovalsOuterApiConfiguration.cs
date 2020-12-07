using SFA.DAS.Commitments.Domain.Api.Configuration;

namespace SFA.DAS.Commitments.Domain.Configuration
{
    public class ApprovalsOuterApiConfiguration : IApprovalsOuterApiConfiguration
    {
        public string PingUrl { get; set; }
        public string Key { get; set; }
        public string BaseUrl { get; set; }
    }
}