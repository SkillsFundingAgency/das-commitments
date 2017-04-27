using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Api.Client.Configuration;

namespace SFA.DAS.Commitments.Api.Client
{
    internal class SecureHttpClient
    {
        private readonly ICommitmentsApiClientConfiguration _configuration;

        public SecureHttpClient(ICommitmentsApiClientConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected SecureHttpClient()
        {
            // So we can mock for testing
        }

        private async Task<AuthenticationResult> GetAuthenticationResult(string clientId, string appKey, string resourceId, string tenant)
        {
            var authority = $"https://login.microsoftonline.com/{tenant}";
            var clientCredential = new ClientCredential(clientId, appKey);
            var context = new AuthenticationContext(authority, true);
            var result = await context.AcquireTokenAsync(resourceId, clientCredential);
            return result;
        }

        public virtual async Task<string> GetAsync(string url)
        {
            var authenticationResult = await GetAuthenticationResult(_configuration.ClientId, _configuration.ClientSecret, _configuration.IdentifierUri, _configuration.Tenant);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsStringAsync();
            }
        }

        public virtual async Task<string> PostAsync(string url, object message)
        {
            string content;
            var authenticationResult = await GetAuthenticationResult(_configuration.ClientId, _configuration.ClientSecret, _configuration.IdentifierUri, _configuration.Tenant);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                client.DefaultRequestHeaders.Add("accept", "application/json");

                var response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }

            return content;
        }

        public async Task<string> PutAsync(string url, string data)
        {
            string content;

            var authenticationResult = await GetAuthenticationResult(_configuration.ClientId, _configuration.ClientSecret, _configuration.IdentifierUri, _configuration.Tenant);

            using (var client = new HttpClient())
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = new StringContent(data, Encoding.UTF8, "application/json")
                };

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                var response = await client.SendAsync(requestMessage);
                content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }

            return content;
        }

        public async Task<string> PatchAsync(string url, string data)
        {
            string content;
            var authenticationResult = await GetAuthenticationResult(_configuration.ClientId, _configuration.ClientSecret, _configuration.IdentifierUri, _configuration.Tenant);
            using (var client = new HttpClient())
            {
                var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = new StringContent(data, Encoding.UTF8, "application/json")
                };

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                var response = await client.SendAsync(requestMessage);
                content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }

            return content;
        }

        public async Task DeleteAsync(string url, string data)
        {
            var authenticationResult = await GetAuthenticationResult(_configuration.ClientId, _configuration.ClientSecret, _configuration.IdentifierUri, _configuration.Tenant);

            using (var client = new HttpClient())
            {
                HttpRequestMessage requestMessage;

                if (string.IsNullOrWhiteSpace(data))
                {
                    requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
                }
                else
                {
                    requestMessage = new HttpRequestMessage(HttpMethod.Delete, url)
                    {
                        Content = new StringContent(data, Encoding.UTF8, "application/json")
                    };
                }

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                var response = await client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
