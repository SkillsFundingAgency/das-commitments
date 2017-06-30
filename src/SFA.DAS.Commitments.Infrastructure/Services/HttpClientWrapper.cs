using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        public string AuthScheme { get; set; }

        public async Task<string> GetString(string url, string accessToken)
        {
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var authScheme = !string.IsNullOrEmpty(AuthScheme)
                        ? AuthScheme : "Bearer";
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, accessToken);
                }

                var response = await client.GetAsync(url);
                EnsureSuccessfulResponse(response);

                return response.Content.ReadAsStringAsync().Result;
            }
        }

        private void EnsureSuccessfulResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }
            throw new HttpException((int)response.StatusCode, $"HTTP exception - ({(int)response.StatusCode}): {response.ReasonPhrase}");
        }
    }
}
