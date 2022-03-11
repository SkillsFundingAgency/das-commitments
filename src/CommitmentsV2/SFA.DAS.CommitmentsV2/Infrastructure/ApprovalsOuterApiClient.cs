using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.Api;

namespace SFA.DAS.CommitmentsV2.Infrastructure
{
    public class ApprovalsOuterApiClient : IApprovalsOuterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApprovalsOuterApiConfiguration _config;
        private readonly ILogger<ApprovalsOuterApiClient> _logger;

        public ApprovalsOuterApiClient (HttpClient httpClient, ApprovalsOuterApiConfiguration config, ILogger<ApprovalsOuterApiClient> logger)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);

            AddHeaders();
        }

        public async Task<TResponse> Get<TResponse>(IGetApiRequest request) 
        {

            _logger.LogInformation("Calling Outer API base {0}, url {1}", _config.BaseUrl, request.GetUrl);
            var response = await _httpClient.GetAsync(request.GetUrl).ConfigureAwait(false);

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                _logger.LogInformation("URL {0} found nothing", request.GetUrl);
                return default;
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("URL {0} returned a response", request.GetUrl);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);    
            }

            _logger.LogInformation("URL {0} returned a response {1}", request.GetUrl, response.StatusCode);
            response.EnsureSuccessStatusCode();
            
            return default;
        }

        private void AddHeaders()
        {
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config.Key);
            _httpClient.DefaultRequestHeaders.Add("X-Version", "1");
        }
    }
}