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
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public Task Ping(Func<string, Task> call)
        {
            var effectiveApiBaseUrl = _config.EffectiveApiBaseUrl.TrimEnd('/');
            var url = $"{effectiveApiBaseUrl}/ping";
             
            return call(url);
        }

        public Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, Func<string, object, Task<ReservationValidationResult>> call)
        {
            var effectiveApiBaseUrl = _config.EffectiveApiBaseUrl.TrimEnd('/');
            var url = $"{effectiveApiBaseUrl}/api/reservations/validate/{request.ReservationId}";

            var data = new
            {
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                request.CourseCode
            };

            return call(url, data);
        }
    }
}