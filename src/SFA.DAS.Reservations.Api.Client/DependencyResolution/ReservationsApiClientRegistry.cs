using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.Configuration;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;
using StructureMap;

namespace SFA.DAS.Reservations.Api.Client.DependencyResolution
{
    public class ReservationsApiClientRegistry : Registry
    {
        public ReservationsApiClientRegistry()
        {
            For<IReservationsApiClient>().Use(ctx => CreateClient(ctx)).Singleton();
        }

        public ReservationsClientApiConfiguration GetConfig(IContext ctx)
        {
            var configurationRepository = ctx.GetInstance<IConfigurationRepository>();
            var environmentName = GetEnvironmentName();
            var configurationOptions =
                new ConfigurationOptions(ConfigurationKeys.ReservationsClientApiConfiguration, environmentName, "1.0");

            var svc = new ConfigurationService(configurationRepository, configurationOptions);

            return svc.Get<ReservationsClientApiConfiguration>();
        }

        private string GetEnvironmentName()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = ConfigurationManager.AppSettings["EnvironmentName"];
            }

            return environment;
        }

        private IReservationsApiClient CreateClient(IContext ctx)
        {
            var config = GetConfig(ctx);
            var log = ctx.GetInstance<ILog>();
            var httpClient =  CreateHttpClient(ctx, config);
            var httpHelper = new HttpHelper(httpClient, log);
            return new ReservationsApiClient(config, httpHelper);
        }

        private HttpClient CreateHttpClient(IContext ctx, ReservationsClientApiConfiguration config)
        {
            if (config.UseStub)
            {
                return new HttpClient();
            }

            if (IsClientCredentialConfiguration(config.ClientId, config.ClientSecret, config.Tenant))
            {
                var adConfig = new ReservationsClientApiConfigurationADAdapter(config);
                var bearerToken = new AzureActiveDirectoryBearerTokenGenerator(adConfig);

                return new HttpClientBuilder()
                    .WithBearerAuthorisationHeader(bearerToken)
                    .WithHandler(new NLog.Logger.Web.MessageHandlers.RequestIdMessageRequestHandler())
                    .WithHandler(new NLog.Logger.Web.MessageHandlers.SessionIdMessageRequestHandler())
                    .WithDefaultHeaders()
                    .Build();
            }
            else
            {
                var httpClient = new HttpClientBuilder()
                   .WithHandler(new NLog.Logger.Web.MessageHandlers.RequestIdMessageRequestHandler())
                   .WithHandler(new NLog.Logger.Web.MessageHandlers.SessionIdMessageRequestHandler())
                   .WithDefaultHeaders()
                   .Build();

                var accessToken =  GetManagedIdentityAuthenticationResult(config.IdentifierUri);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return httpClient;
            }
        }
        private bool IsClientCredentialConfiguration(string clientId, string clientSecret, string tenant)
        {
            return !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret) && !string.IsNullOrWhiteSpace(tenant);
        }
        private string GetManagedIdentityAuthenticationResult(string resource)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            return azureServiceTokenProvider.GetAccessTokenAsync(resource).Result;
        }
    }
}
