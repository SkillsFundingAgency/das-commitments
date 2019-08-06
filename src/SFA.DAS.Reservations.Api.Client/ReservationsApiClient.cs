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
            var uri = $"api/reservations/validate/{request.ReservationId}";
            return _client.Get<ValidationResult>(uri,
                new
                {
                    StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                    request.CourseCode
                },
                cancellationToken);
        }
    }
}
