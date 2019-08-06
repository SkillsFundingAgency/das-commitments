using SFA.DAS.Reservations.Api.Client;
using SFA.DAS.Reservations.Api.Types;
using StructureMap;

namespace SFA.DAS.Reservations.Api.Client.DependencyResolution
{
    public class ReservationsApiClientRegistry : Registry
    {
        public ReservationsApiClientRegistry()
        {
            For<IReservationsApiClient>().Use<ReservationsApiClient>().Singleton();
            For<IReservationHelper>().Use<ReservationHelper>().Singleton();
        }
    }
}
