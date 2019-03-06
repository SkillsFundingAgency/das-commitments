using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public class CommitmentsApiClientFactory : ICommitmentsApiClientFactory
    {
        private readonly AzureActiveDirectoryClientConfiguration _config;

        public CommitmentsApiClientFactory(AzureActiveDirectoryClientConfiguration config)
        {
            _config = config;
        }
        public ICommitmentsApiClient CreateClient()
        {
            var httpClientFactory = new AzureActiveDirectoryHttpClientFactory(_config);
            var httpClient = httpClientFactory.CreateHttpClient();
            var restHttpClient = new RestHttpClient(httpClient);
            return new CommitmentsApiClient(restHttpClient);
        }
    }
}
