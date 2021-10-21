using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Infrastructure
{
    public class LevyTransferMatchingClient : ILevyTransferMatchingApiClient
    {
        private readonly HttpClient _httpClient;

        public LevyTransferMatchingClient(HttpClient httpClient, IAccessTokenProvider accessTokenProvider)
        {
            _httpClient = httpClient;
            if (!_httpClient.BaseAddress.IsLoopback)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenProvider.GetAccessToken());
            }
        }
        
        public async Task<PledgeApplication> GetPledgeApplication(int id)
        {
            var response = await _httpClient.GetAsync($"applications/{id}").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<PledgeApplication>(json);
        }
    }
}
