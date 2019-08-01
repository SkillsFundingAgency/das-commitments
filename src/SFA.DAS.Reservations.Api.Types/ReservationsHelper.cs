using System;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationsHelper : IReservationHelper
    {
        public Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, Func<string, object, Task<ReservationValidationResult>> call)
        {
            var url = $"api/accounts/{request.AccountId}/reservations/{request.ReservationId}";

            var data = new
            {
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                request.CourseCode
            };

            return call(url, data);
        }
    }
}