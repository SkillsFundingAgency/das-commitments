using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Api.Types
{
    public interface IReservationsApiClient
    {
        Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, CancellationToken cancellationToken);
        Task<BulkCreateReservationsResult> BulkCreateReservations(long accountLegalEntity, uint count, CancellationToken cancellationToken);
    }
}
