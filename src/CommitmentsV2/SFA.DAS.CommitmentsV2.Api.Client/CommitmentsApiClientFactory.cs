using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.CommitmentsV2.Api.Client.Http;
using SFA.DAS.Http;
using System;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public class CommitmentsApiClientFactory : ICommitmentsApiClientFactory
    {
        private readonly CommitmentsClientApiConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public CommitmentsApiClientFactory(CommitmentsClientApiConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }
        
        public ICommitmentsApiClient CreateClient()
        {
            var value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (value == "Development")
            {
                var httpClientBuilder = new HttpClientBuilder();
                var httpClient = httpClientBuilder
               .WithDefaultHeaders()
               .WithBearerAuthorisationHeader(new DAS.Http.TokenGenerators.GenericJwtBearerTokenGenerator(new JwtConfig()))
               .Build();

                httpClient.BaseAddress = new Uri(_configuration.ApiBaseUrl);

                var restHttpClient = new CommitmentsRestHttpClient(httpClient, _loggerFactory);
                return new CommitmentsApiClient(restHttpClient);

            }
            else
            {
                var httpClientFactory = new ManagedIdentityHttpClientFactory(_configuration, _loggerFactory);
                var httpClient = httpClientFactory.CreateHttpClient();
                var restHttpClient = new CommitmentsRestHttpClient(httpClient, _loggerFactory);
                var apiClient = new CommitmentsApiClient(restHttpClient);

                return apiClient;
            }
        }

        public class JwtConfig : DAS.Http.Configuration.IGenericJwtClientConfiguration
        {
            public string Issuer => "dummyissuer";

            public string Audience => "dummyaudience";

            public string ClientSecret => "dummyaudience";

            public int TokenExpirySeconds => 5000;
        }
    }
}
