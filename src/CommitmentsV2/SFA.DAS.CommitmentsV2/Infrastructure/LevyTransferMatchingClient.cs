using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Infrastructure
{
    public class LevyTransferMatchingClient : ILevyTransferMatchingApiClient
    {
        private readonly LevyTransferMatchingApiConfiguration _configuration;

        public LevyTransferMatchingClient(LevyTransferMatchingApiConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<PledgeApplication> GetPledgeApplication(int id)
        {
            var baseUrl = _configuration.BaseUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            var baseUri = new Uri(baseUrl);

            var accessToken = !baseUri.IsLoopback ? GetAccessToken() : string.Empty;

            var httpClient = new HttpClient {BaseAddress = baseUri };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("X-Version", "1.0");

            var response = await httpClient.GetAsync($"applications/{id}").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<PledgeApplication>(json);
        }

        protected string GetAccessToken()
        {
            var tokenProvider = new AzureServiceTokenProvider();
            var getTokenTask = tokenProvider.GetAccessTokenAsync(_configuration.Identifier);
            return getTokenTask.GetAwaiter().GetResult();
        }
    }

    public interface ILevyTransferMatchingApiClient
    {
        Task<PledgeApplication> GetPledgeApplication(int id);
    }

    public class PledgeApplication
    {
        public long SenderEmployerAccountId { get; set; }
        public long ReceiverEmployerAccountId { get; set; }
        public ApplicationStatus Status { get; set; }

        public enum ApplicationStatus : byte
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2,
            Accepted = 3,
            Declined = 4
        }
    }
}
