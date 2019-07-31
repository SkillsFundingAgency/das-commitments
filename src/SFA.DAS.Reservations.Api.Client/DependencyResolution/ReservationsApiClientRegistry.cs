using System;
using System.Net.Http;
using SFA.DAS.Configuration;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;
using StructureMap;

namespace SFA.DAS.Reservation.Api.Client.DependencyResolution
{
    public class ReservationsApiClientRegistry : Registry
    {
        public ReservationsApiClientRegistry()
        {
            For<IReservationsApiClient>().Use<ReservationsApiClient>().Singleton();
            For<IReservationHelper>().Use<ReservationsHelper>().Singleton();
        }
    }
}
