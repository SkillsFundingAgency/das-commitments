using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Domain.Api;
using SFA.DAS.Commitments.Domain.Api.Configuration;
using SFA.DAS.Commitments.Domain.Api.Requests;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Api
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IApprovalsOuterApiConfiguration _config;

        public ApiClient (HttpClient httpClient, IApprovalsOuterApiConfiguration config)
        {
            _config = config;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config.BaseUrl);
            
        }
        
        public async Task<TResponse> Get<TResponse>(IGetApiRequest request) 
        {
            
            AddHeaders();

            var response = await _httpClient.GetAsync(request.GetUrl).ConfigureAwait(false);

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                return default;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);    
            }
            
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