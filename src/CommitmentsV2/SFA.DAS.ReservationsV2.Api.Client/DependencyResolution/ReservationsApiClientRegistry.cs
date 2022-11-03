using SFA.DAS.Reservations.Api.Types;
using StructureMap;

namespace SFA.DAS.ReservationsV2.Api.Client.DependencyResolution
{
    public class ReservationsApiClientRegistry : Registry
    {
        public ReservationsApiClientRegistry()
        {
            For<IReservationsApiClient>().Use(c => c.GetInstance<IReservationsApiClientFactory>().CreateClient()).Singleton();
            For<IReservationsApiClientFactory>().Use<ReservationsApiClientFactory>();
        } 
    }
}
