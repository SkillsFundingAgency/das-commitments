using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.Client
{
    public class CommitmentsApi : ICommitmentsApi
    {
        private readonly string _baseUrl;

        public CommitmentsApi(string baseUrl)
        {
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));
            _baseUrl = baseUrl;
        }

        public async Task<List<CommitmentListItem>> GetProviderCommitments(long providerId)
        {
            var url = $"{_baseUrl}api/provider/{providerId}/commitments";

            return await GetCommitments(url);
        }

        public async Task<List<CommitmentListItem>> GetEmployerCommitments(long employerAccountId)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments";

            return await GetCommitments(url);
        }

        public async Task<Commitment> GetProviderCommitment(long providerId, long commitmentId)
        {
            var url = $"{_baseUrl}api/provider/{providerId}/commitments/{commitmentId}";

            return await GetCommitment(url);
        }

        public async Task<Commitment> GetEmployerCommitment(long employerAccountId, long commitmentId)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}";

            return await GetCommitment(url);
        }

        public async Task PostEmployerCommitment(long employerAccountId, Commitment commitment)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments";

            var data = JsonConvert.SerializeObject(commitment);

            await PostAsync(url, data);
        }

        public async Task<Apprenticeship> GetProviderApprenticeship(long providerId, long commitmentId, long apprenticeshipId)
        {
            var url = $"{_baseUrl}api/provider/{providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}";

            return await GetApprenticeship(url);
        }

        private async Task<List<CommitmentListItem>> GetCommitments(string url)
        {
            var content = await GetAsync(url);

            return JsonConvert.DeserializeObject<List<CommitmentListItem>>(content);
        }

        private async Task<Commitment> GetCommitment(string url)
        {
            var content = await GetAsync(url);

            return JsonConvert.DeserializeObject<Commitment>(content);
        }

        private async Task<Apprenticeship> GetApprenticeship(string url)
        {
            var content = await GetAsync(url);

            return JsonConvert.DeserializeObject<Apprenticeship>(content);
        }

        private async Task<string> GetAsync(string url)
        {
            var content = "";

            try
            {
                using (var client = new HttpClient())
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                    // Add custom headers
                    //requestMessage.Headers.Add("User-Agent", "User-Agent-Here");

                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("WRAP", "bigAccessToken");
                    var response = await client.SendAsync(requestMessage);
                    content = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (HttpRequestException ex)
            {
                throw;
            }

            return content;
        }

        private async Task<string> PostAsync(string url, string data)
        {
            var content = "";

            try
            {
                using (var client = new HttpClient())
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(data)
                    };

                    // Add custom headers
                    //requestMessage.Headers.Add("User-Agent", "User-Agent-Here");

                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("WRAP", "bigAccessToken");
                    var response = await client.SendAsync(requestMessage);
                    content = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (HttpRequestException ex)
            {
                throw;
            }

            return content;
        }

        private async Task<string> PutAsync(string url, string data)
        {
            var content = "";

            try
            {
                using (var client = new HttpClient())
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Put, url)
                    {
                        Content = new StringContent(data)
                    };

                    // Add custom headers
                    //requestMessage.Headers.Add("User-Agent", "User-Agent-Here");

                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("WRAP", "bigAccessToken");
                    var response = await client.SendAsync(requestMessage);
                    content = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (HttpRequestException ex)
            {
                throw;
            }

            return content;
        }
    }
}