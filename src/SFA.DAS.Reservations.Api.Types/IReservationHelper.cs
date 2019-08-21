using System;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Api.Types
{
    public interface IReservationHelper
    {
        Task<ReservationValidationResult> ValidateReservation(
            ValidationReservationMessage request,
            Func<string, object, Task<ReservationValidationResult>> call);

        Task<BulkCreateReservationsResult> BulkCreateReservations(
            long accountLegalEntityId,
            BulkCreateReservationsRequest request,
            Func<string, BulkCreateReservationsRequest, Task<BulkCreateReservationsResult>> call);
    }
}