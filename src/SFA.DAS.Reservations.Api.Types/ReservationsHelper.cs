using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationsHelper : IReservationHelper
    {
        private readonly ReservationsClientApiConfiguration _config;

        public ReservationsHelper(ReservationsClientApiConfiguration config)
        {
            _config = config;
        }

        public Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, Func<string, object, Task<ReservationValidationResult>> call)
        {
            var url = $"{_config.ApiBaseUrl}/api/accounts/{request.AccountId}/reservations/{request.ReservationId}";

            var data = new
            {
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                request.CourseCode
            };

            return call(url, data);
        }
    }
}