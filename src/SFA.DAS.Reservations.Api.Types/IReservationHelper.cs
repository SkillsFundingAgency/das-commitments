using System;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Api.Types
{
    public interface IReservationHelper
    {
        Task<ReservationValidationResult> ValidateReservation(
            ValidationReservationMessage request,
            Func<string, object, Task<ReservationValidationResult>> call);

        Task<BulkCreateReservationsResult> BulkCreateReservations(long accountLegalEntityId, uint count,
            Func<string, Task<BulkCreateReservationsResult>> call);
    }
}