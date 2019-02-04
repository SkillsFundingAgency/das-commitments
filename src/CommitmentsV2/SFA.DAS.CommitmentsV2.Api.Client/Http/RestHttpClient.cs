using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.Client.Http
{
    public class RestHttpClient : IRestHttpClient
    {
        private readonly HttpClient _httpClient;
        
        public RestHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> Get<T>(Uri uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            var response = await GetResponse(uri, queryData, cancellationToken);
            return await response.Content.ReadAsAsync<T>(cancellationToken);
        }
        
        public Task<T> Get<T>(string uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            return Get<T>(new Uri(uri, UriKind.RelativeOrAbsolute), queryData, cancellationToken);
        }

        public async Task<string> Get(Uri uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            var response = await GetResponse(uri, queryData, cancellationToken);
            return await response.Content.ReadAsStringAsync();
        }
        
        public Task<string> Get(string uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            return Get(new Uri(uri, UriKind.RelativeOrAbsolute), queryData, cancellationToken);
        }

        private async Task<HttpResponseMessage> GetResponse(Uri uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            if (queryData != null)
            {
                uri = new Uri(AddQueryString(uri.ToString(), queryData), UriKind.RelativeOrAbsolute);
            }

            var response = await _httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateClientException(response);
            }

            return response;
        }
        
        private string AddQueryString(string uri, object queryData)
        {
            var queryDataDictionary = queryData.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(queryData)?.ToString() ?? string.Empty);
            return QueryHelpers.AddQueryString(uri, queryDataDictionary);
        }

        public async Task<Exception> CreateClientException(HttpResponseMessage httpResponseMessage)
        {
            var errorString = await httpResponseMessage.Content.ReadAsStringAsync();

            var errorDetails = JsonConvert.DeserializeObject<ErrorDetails>(errorString);

            if (errorDetails != null && errorDetails.Message != null)
            {
                return new ApiException(errorDetails.ErrorCode, errorDetails.Message);
            }

            return new RestHttpClientException(httpResponseMessage, errorString);
        }



    }
}