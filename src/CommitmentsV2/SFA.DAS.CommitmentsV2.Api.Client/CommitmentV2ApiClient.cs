using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Client.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public class CommitmentV2ApiClient : ICommitmentV2ApiClient
    {
        private readonly IRestHttpClient _client;

        public CommitmentV2ApiClient(IRestHttpClient client)
        {
            _client = client;
        }
        public async Task<bool> HealthCheck()
        {
            var result = await _client.Get("api/health-check");
            if (result?.Equals("Healthy", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return true;
            }

            return false;
        }
    }
}
