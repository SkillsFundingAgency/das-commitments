using SFA.DAS.Http.Configuration;

namespace SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;

public class ProviderRelationshipsApiConfiguration : IManagedIdentityClientConfiguration
{
    public string ApiBaseUrl { get; set; }
    public string IdentifierUri { get; set; }
}