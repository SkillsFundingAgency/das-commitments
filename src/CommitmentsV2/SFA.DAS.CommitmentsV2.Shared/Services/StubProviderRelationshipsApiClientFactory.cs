using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Api.Client.Http;

namespace SFA.DAS.CommitmentsV2.Shared.Services
{
    public class StubProviderRelationshipsApiClientFactory : IProviderRelationshipsApiClientFactory
    {
        public IProviderRelationshipsApiClient CreateApiClient()
        {
            return new StubProviderRelationshipsApiClient();
        }
    }
}
