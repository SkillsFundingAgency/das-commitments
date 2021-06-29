using SFA.DAS.Http.Configuration;

namespace SFA.DAS.CommitmentsV2.Api.Client.Configuration
{
    public class CommitmentsClientApiConfiguration : IManagedIdentityClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string IdentifierUri { get; set; }
    }
}
