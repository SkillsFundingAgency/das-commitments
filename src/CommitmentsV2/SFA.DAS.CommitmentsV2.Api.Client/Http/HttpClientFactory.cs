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
                //.WithBearerAuthorisationHeader(new AzureADBearerTokenGenerator(_configuration))
                .Build();
            
            httpClient.BaseAddress = new Uri(_configuration.ApiBaseUrl);

            return httpClient;
        }
    }

    public class AzureActiveDirectoryClientConfiguration : IAzureADClientConfiguration
    {
        public AzureActiveDirectoryClientConfiguration()
        {
            ClientId = "85f054e9-df35-4646-abb6-7a2efb4a8d48";
            ClientSecret = "mBjbdtTFO/LelO8vvTWgEy6/B97Z3J3YUHIo+Ewl7OQ=";
            IdentifierUri = "https://citizenazuresfabisgov.onmicrosoft.com/das-provider-relationships-api";
            Tenant = "citizenazuresfabisgov.onmicrosoft.com";
        }
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string IdentifierUri { get; }
    }
}