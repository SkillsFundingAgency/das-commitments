using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types.Types;

namespace SFA.DAS.Reservations.Api.Types
{
    public interface IReservationHelper
    {
        Task<ReservationValidationResult> ValidateReservation(
            ValidationReservationMessage request,
            Func<string, object, Task<ReservationValidationResult>> call);
    }

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
