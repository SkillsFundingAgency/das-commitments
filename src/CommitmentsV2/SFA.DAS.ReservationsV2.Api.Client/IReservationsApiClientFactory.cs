using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.ReservationsV2.Api.Client
{
    public interface IReservationsApiClientFactory
    {
        IReservationsApiClient CreateClient();
    }
}
