using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public class CommitmentsApiClientFactory : ICommitmentsApiClientFactory
    {
        private readonly CommitmentsClientApiConfiguration _config;

        public CommitmentsApiClientFactory(CommitmentsClientApiConfiguration config)
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
