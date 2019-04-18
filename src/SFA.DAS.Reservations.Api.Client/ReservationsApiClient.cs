using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.Reservations.Api.Client
{
    public class ReservationsApiClient : IReservationsApiClient
    {
        private readonly IRestHttpClient _client;

        public ReservationsApiClient(IRestHttpClient client)
        {
            _client = client;
        }

        public Task<ValidationResult> ValidateReservation(ValidationReservationMessage request, CancellationToken cancellationToken)
        {
            return _client.PutAsJson<ValidationReservationMessage, ValidationResult>($"api/accounts/{request.AccountId}/reservations/{request.ReservationId}", request, cancellationToken);
        }
    }
}
