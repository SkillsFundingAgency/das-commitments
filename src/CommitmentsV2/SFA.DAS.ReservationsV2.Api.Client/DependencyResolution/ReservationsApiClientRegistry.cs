using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;
using StructureMap;

namespace SFA.DAS.ReservationsV2.Api.Client.DependencyResolution
{
    /// <summary>
    ///     Presents the reservation configuration as an IReservationHelper
    /// </summary>
    public class ReservationsClientApiConfiguration : Reservations.Api.Types.Configuration.ReservationsClientApiConfiguration,
        IAzureActiveDirectoryClientConfiguration
    {
    }

    public class ReservationsApiClientRegistry : Registry
    {
        public ReservationsApiClientRegistry()
        {
            For<IReservationsApiClient>().Use(ctx => CreateClient(ctx)).Singleton();
            For<IReservationHelper>().Use<ReservationsHelper>().Singleton();
        } 

        private IReservationsApiClient CreateClient(IContext ctx)
        {
            var config = GetConfig(ctx);
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
