using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationHelper : IReservationHelper
    {
        private readonly ReservationsClientApiConfiguration _config;

        public ReservationHelper(ReservationsClientApiConfiguration config)
        {
            _config = config;
        }

        public Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, Func<string, object, Task<ReservationValidationResult>> call)
        {
            var url = $"{_config.EffectiveApiBaseUrl}/api/reservations/validate/{request.ReservationId}?courseCode={request.CourseCode}&startDate={request.StartDate}";

            var data = new
            {
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                request.CourseCode
            };

            return call(url, data);
        }
    }
}