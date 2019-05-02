using System;
using System.Net.Http;
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
            var config = ctx.GetInstance<ReservationsClientApiConfiguration>();

            HttpClient httpClient;

            if (config.UseStub)
            {
                httpClient = new HttpClient {BaseAddress = new Uri("https://sfa-stub-reservations.herokuapp.com/") };
            }
            else
            {
                var httpClientFactory = new AzureActiveDirectoryHttpClientFactory(config);
                httpClient = httpClientFactory.CreateHttpClient();
            }

            var restHttpClient = new RestHttpClient(httpClient);
            return new ReservationsApiClient(restHttpClient);
        }
    }
}
