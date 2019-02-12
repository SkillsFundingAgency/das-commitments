using System;
using System.Net.Http;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;

namespace SFA.DAS.CommitmentsV2.Api.Client.Http
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly AzureActiveDirectoryClientConfiguration _configuration;

        public HttpClientFactory(AzureActiveDirectoryClientConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClientBuilder()
                .WithDefaultHeaders()
                .WithBearerAuthorisationHeader(new AzureADBearerTokenGenerator(_configuration))
                .Build();
            
            httpClient.BaseAddress = new Uri(_configuration.ApiBaseUrl);

            return httpClient;
        }
    }

    public class AzureActiveDirectoryClientConfiguration : IAzureADClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
    }
}