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
            var httpClientFactory = new AzureActiveDirectoryHttpClientFactory(config);
            var httpClient = httpClientFactory.CreateHttpClient();
            var restHttpClient = new RestHttpClient(httpClient);
            return new ReservationsApiClient(restHttpClient);
        }
    }
}
