using System;
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
            var value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (value == "Development")
            {
                var _loggerFactory = ctx.GetInstance<ILoggerFactory>();
                var httpClientBuilder = new HttpClientBuilder();

                if (_loggerFactory != null)
                {
                    httpClientBuilder.WithLogging(_loggerFactory);
                }

                var httpClient = httpClientBuilder
                    .WithDefaultHeaders()
                    .Build();

                httpClient.BaseAddress = new Uri(config.ApiBaseUrl);

                return httpClient;
            }

            if (config.UseStub)
            {
                return new HttpClient();
            }

            var loggerFactory = ctx.GetInstance<ILoggerFactory>();
            var activeDirectoryConfig = new ReservationsClientApiConfigurationADAdapter(config);
            var httpClientFactory = new AzureActiveDirectoryHttpClientFactory(activeDirectoryConfig, loggerFactory);
            return httpClientFactory.CreateHttpClient();
        }

        private static ReservationsClientApiConfiguration GetConfig(IContext context)
        {
            var configuration = context.GetInstance<IConfiguration>();
            var configSection = configuration.GetSection(ConfigurationKeys.ReservationsClientApiConfiguration);
            return configSection.Get<ReservationsClientApiConfiguration>();
        }
    }
}
