using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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

        public async Task CreateEmployerCommitment(long employerAccountId, Commitment commitment)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments";

            await PostCommitment(url, commitment);
        }

        public async Task<List<CommitmentListItem>> GetEmployerCommitments(long employerAccountId)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments";

            return await GetCommitments(url);
        }

        public async Task<Commitment> GetEmployerCommitment(long employerAccountId, long commitmentId)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}";

            return await GetCommitment(url);
        }

        public async Task<Apprenticeship> GetEmployerApprenticeship(long employerAccountId, long commitmentId, long apprenticeshipId)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}";

            return await GetApprenticeship(url);
        }

        public async Task PatchEmployerCommitment(int employerAccountId, int commitmentId, CommitmentStatus status)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}";

            await PatchCommitment(url, status);
        }

        public async Task UpdateEmployerApprenticeship(long employerAccountId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            var url = $"{_baseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}";

            await PutApprenticeship(url, apprenticeship);
        }

        public async Task<Apprenticeship> GetProviderApprenticeship(long providerId, long commitmentId, long apprenticeshipId)
        {
            var url = $"{_baseUrl}api/provider/{providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}";

            return await GetApprenticeship(url);
        }

        public async Task CreateProviderApprenticeship(long providerId, long commitmentId, Apprenticeship apprenticeship)
        {
            var url = $"{_baseUrl}api/provider/{providerId}/commitments/{commitmentId}/apprenticeships";

            await PostApprenticeship(url, apprenticeship);
        }

        public async Task UpdateProviderApprenticeship(long providerId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            var url = $"{_baseUrl}api/provider/{providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}";

            await PutApprenticeship(url, apprenticeship);
        }

        public async Task<List<CommitmentListItem>> GetProviderCommitments(long providerId)
        {
            var url = $"{_baseUrl}api/provider/{providerId}/commitments";

            return await GetCommitments(url);
        }

       
        public async Task<Commitment> GetProviderCommitment(long providerId, long commitmentId)
        {
            var url = $"{_baseUrl}api/provider/{providerId}/commitments/{commitmentId}";

            return await GetCommitment(url);
        }

        private async Task<Commitment> PostCommitment(string url, Commitment commitment)
        {
            var data = JsonConvert.SerializeObject(commitment);
            var content = await PostAsync(url, data);

            return JsonConvert.DeserializeObject<Commitment>(content);
        }

        private async Task PatchCommitment(string url, CommitmentStatus status)
        {
            var data = JsonConvert.SerializeObject(status);
            await PatchAsync(url, data);
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

        private async Task PutApprenticeship(string url, Apprenticeship apprenticeship)
        {
            var data = JsonConvert.SerializeObject(apprenticeship);
            var content = await PutAsync(url, data);
        }

        private async Task<Apprenticeship> PostApprenticeship(string url, Apprenticeship apprenticeship)
        {
            var data = JsonConvert.SerializeObject(apprenticeship);
            var content = await PostAsync(url, data);

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
                        Content = new StringContent(data, Encoding.UTF8, "application/json")
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
                        Content = new StringContent(data, Encoding.UTF8, "application/json")
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

        private async Task<string> PatchAsync(string url, string data)
        {
            var content = "";

            try
            {
                using (var client = new HttpClient())
                {
                    var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                    {
                        Content = new StringContent(data, Encoding.UTF8, "application/json")
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