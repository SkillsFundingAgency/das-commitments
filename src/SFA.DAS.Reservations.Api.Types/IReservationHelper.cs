using System;
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
}