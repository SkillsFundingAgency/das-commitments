using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Client.Configuration;
using StructureMap;

namespace SFA.DAS.Reservations.Api.Client.DependencyResolution
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
            var loggerFactory = ctx.GetInstance<ILoggerFactory>();

            HttpClient httpClient;

            if (config.UseStub)
            {
                httpClient = new HttpClient {BaseAddress = new Uri("https://sfa-stub-reservations.herokuapp.com/") };
            }
            else
            {
                var httpClientFactory = new AzureActiveDirectoryHttpClientFactory(config, loggerFactory);
                httpClient = httpClientFactory.CreateHttpClient();
            }

            var restHttpClient = new RestHttpClient(httpClient);
            return new ReservationsApiClient(restHttpClient);
        }

        private static ReservationsClientApiConfiguration GetConfig(IContext context)
        {
            var configuration = context.GetInstance<IConfiguration>();
            var configSection = configuration.GetSection(ConfigurationKeys.ReservationsClientApiConfiguration);
            return configSection.Get<ReservationsClientApiConfiguration>();
        }
    }
}
