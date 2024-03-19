using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.Infrastructure
{
    public class ApprovalsOuterApiClient : IApprovalsOuterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy _asyncRetryPolicy;
        private readonly ApprovalsOuterApiConfiguration _config;
        private readonly ILogger<ApprovalsOuterApiClient> _logger;

        public ApprovalsOuterApiClient(HttpClient httpClient, ApprovalsOuterApiConfiguration config, ILogger<ApprovalsOuterApiClient> logger)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _asyncRetryPolicy = GetRetryPolicy();
        }

        public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
        {

            _logger.LogInformation("Calling Outer API base {0}, url {1}", _config.BaseUrl, request.GetUrl);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);

            AddHeaders(httpRequestMessage);

            var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

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

        public async Task<TResponse> GetWithRetry<TResponse>(IGetApiRequest request)
        {
            return await _asyncRetryPolicy.ExecuteAsync(async () => await Get<TResponse>(request));
        }

        public async Task<TResponse> PostAsync<TResponse>(IPostApiRequest request)
        {
            return await PostAsync<object, TResponse>(request);
        }

        public async Task<TResponse> PostAsync<TData, TResponse>(IPostApiRequest<TData> request)
        {
            _logger.LogInformation("Calling Outer API base {0}, url {1}", _config.BaseUrl, request.PostUrl);

            var jsonRequest = JsonConvert.SerializeObject(request.Data);
            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(request.PostUrl, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("URL {0} returned a response", request.PostUrl);
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(jsonResponse);
            }

            _logger.LogInformation("URL {0} returned a response {1}", request.PostUrl, response.StatusCode);
            response.EnsureSuccessStatusCode();

            return default;
        }

        private void AddHeaders(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key);
            httpRequestMessage.Headers.Add("X-Version", "1");
        }

        private AsyncRetryPolicy GetRetryPolicy()
        {
            var maxRetryAttempts = 3;
            var pauseBetweenFailures = TimeSpan.FromSeconds(2);

            return Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);
        }
    }
}