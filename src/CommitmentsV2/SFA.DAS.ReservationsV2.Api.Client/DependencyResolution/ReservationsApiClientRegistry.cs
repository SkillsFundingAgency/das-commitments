using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Client.DependencyResolution;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;
using StructureMap;

namespace SFA.DAS.ReservationsV2.Api.Client.DependencyResolution
{
    public class ReservationsApiClientRegistry : Registry
    {
        public ReservationsApiClientRegistry()
        {
            For<IReservationsApiClient>().Use(ctx => CreateClient(ctx)).Singleton();
        } 

        private IReservationsApiClient CreateClient(IContext ctx)
        {
            var config = GetConfig(ctx);
            var httpClient = CreateHttpClient(ctx, config);
            var restHttpClient = new RestHttpClient(httpClient);
            var httpHelper = new HttpHelper(restHttpClient, ctx.GetInstance<ILogger<ReservationsApiClient>>());
            return new ReservationsApiClient(config, httpHelper);
        }

        private HttpClient CreateHttpClient(IContext ctx, ReservationsClientApiConfiguration config)
        {
            if (config.UseStub)
            {
                return new HttpClient();
            }

            var loggerFactory = ctx.GetInstance<ILoggerFactory>();

            IHttpClientFactory httpClientFactory;

            if (IsClientCredentialConfiguration(config.ClientId, config.ClientSecret, config.Tenant))
            {
                var activeDirectoryConfig = new ReservationsClientApiConfigurationADAdapter(config);
                httpClientFactory = new AzureActiveDirectoryHttpClientFactory(activeDirectoryConfig, loggerFactory);
            }
            else
            {
                var miConfig = new ReservationsClientApiConfigurationMIAdapter(config);
                httpClientFactory = new ManagedIdentityHttpClientFactory(miConfig, loggerFactory);
            }

            return httpClientFactory.CreateHttpClient();
        }

        private bool IsClientCredentialConfiguration(string clientId, string clientSecret, string tenant)
        {
            return !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret) && !string.IsNullOrWhiteSpace(tenant);
        }
        private static ReservationsClientApiConfiguration GetConfig(IContext context)
        {
            var configuration = context.GetInstance<IConfiguration>();
            var configSection = configuration.GetSection(ConfigurationKeys.ReservationsClientApiConfiguration);
            return configSection.Get<ReservationsClientApiConfiguration>();
        }
    }
}
