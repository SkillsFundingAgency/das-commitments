using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Client.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public class CommitmentsApiClient : ICommitmentsApiClient
    {
        private readonly IRestHttpClient _client;

        public CommitmentsApiClient(IRestHttpClient client)
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

        public Task<LegalEntity> GetLegalEntity(GetLegalEntity request)
        {
            return _client.Get<LegalEntity>($"api/accountlegalentity/{request.AccountLegalEntityId}");
        }

        public Task<string> SecureCheck()
        {
            return _client.Get("api/test");
        }

        public Task<string> SecureEmployerCheck()
        {
            return _client.Get("api/test/employer");
        }

        public Task<string> SecureProviderCheck()
        {
            return _client.Get("api/test/provider");
        }
    }
}
