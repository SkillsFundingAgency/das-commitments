using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.ReservationsV2.Api.Client
{
    public class ReservationsApiClient : IReservationsApiClient
    {
        private readonly IRestHttpClient _client;
        private readonly IReservationHelper _reservationHelper;

        public ReservationsApiClient(IRestHttpClient client, IReservationHelper reservationHelper)
        {
            _client = client;
            _reservationHelper = reservationHelper;
        }

        public Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, CancellationToken cancellationToken)
        {
            return _reservationHelper.ValidateReservation(request, (url, data) =>
                _client.Get<ReservationValidationResult>(url, data, cancellationToken));
        }
    }
}
