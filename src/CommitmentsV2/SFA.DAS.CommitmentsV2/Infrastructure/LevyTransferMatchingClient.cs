using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Infrastructure
{
    public class LevyTransferMatchingClient : ILevyTransferMatchingApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly LevyTransferMatchingApiConfiguration _config;
        private readonly ILogger<LevyTransferMatchingClient> _logger;

        public LevyTransferMatchingClient(HttpClient httpClient, IAccessTokenProvider accessTokenProvider, LevyTransferMatchingApiConfiguration config, ILogger<LevyTransferMatchingClient> logger)
        {
            _httpClient = httpClient;
            _accessTokenProvider = accessTokenProvider;
            _config = config;
            _logger = logger;

            AddHeaders();
        }

        public async Task<PledgeApplication> GetPledgeApplication(int id)
        {
            _logger.LogInformation($"Getting pledge application {id}");

            if (!_httpClient.BaseAddress.IsLoopback)
            {
                var token = await _accessTokenProvider.GetAccessToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.GetAsync($"applications/{id}").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<PledgeApplication>(json);
        }

        private void AddHeaders()
        {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-Version", "1.0");
        }
    }
}
