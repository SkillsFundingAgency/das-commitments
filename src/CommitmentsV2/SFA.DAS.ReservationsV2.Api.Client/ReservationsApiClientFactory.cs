using System.Net.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Client.DependencyResolution;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;
using SFA.DAS.ReservationsV2.Api.Client.DependencyResolution;

namespace SFA.DAS.ReservationsV2.Api.Client
{
   public class ReservationsApiClientFactory : IReservationsApiClientFactory
    {
   
        private readonly ReservationsClientApiConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public ReservationsApiClientFactory(ReservationsClientApiConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IReservationsApiClient CreateClient()
        {
            var httpClient = CreateHttpClient();
            var restHttpClient = new RestHttpClient(httpClient);
            var httpHelper = new HttpHelper(restHttpClient, _loggerFactory.CreateLogger<ReservationsApiClient>());
            return new ReservationsApiClient(_configuration, httpHelper);
        }

        private HttpClient CreateHttpClient()
        {
            if (_configuration.UseStub)
            {
                return new HttpClient();
            }

            IHttpClientFactory httpClientFactory;

            if (IsClientCredentialConfiguration(_configuration.ClientId, _configuration.ClientSecret, _configuration.Tenant))
            {
                var activeDirectoryConfig = new ReservationsClientApiConfigurationADAdapter(_configuration);
                httpClientFactory = new AzureActiveDirectoryHttpClientFactory(activeDirectoryConfig, _loggerFactory);
            }
            else
            {
                var miConfig = new ReservationsClientApiConfigurationMIAdapter(_configuration);
                httpClientFactory = new ManagedIdentityHttpClientFactory(miConfig, _loggerFactory);
            }

            return httpClientFactory.CreateHttpClient();
        }

        private bool IsClientCredentialConfiguration(string clientId, string clientSecret, string tenant)
        {
            return !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret) && !string.IsNullOrWhiteSpace(tenant);
        }
    }
}
