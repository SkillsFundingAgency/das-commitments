using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;
using StructureMap;

namespace SFA.DAS.ReservationsV2.Api.Client.DependencyResolution
{
    public class ReservationsApiClientRegistry : Registry
    {
        public ReservationsApiClientRegistry()
        {
            For<ReservationsClientApiConfiguration>().Use(ctx => GetConfig(ctx)).Singleton();
            For<IReservationsApiClient>().Use(ctx => CreateClient(ctx)).Singleton();
            For<IReservationHelper>().Use<ReservationHelper>().Singleton();
        } 

        private IReservationsApiClient CreateClient(IContext ctx)
        {
            var config = ctx.GetInstance<ReservationsClientApiConfiguration>();
            var loggerFactory = ctx.GetInstance<ILoggerFactory>();
            var reservationHelper = ctx.GetInstance<IReservationHelper>();

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
            return new ReservationsApiClient(restHttpClient, reservationHelper);
        }

        private static ReservationsClientApiConfiguration GetConfig(IContext context)
        {
            var configuration = context.GetInstance<IConfiguration>();
            var configSection = configuration.GetSection(ConfigurationKeys.ReservationsClientApiConfiguration);
            return configSection.Get<ReservationsClientApiConfiguration>();
        }
    }
}
