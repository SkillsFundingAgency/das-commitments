using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Api.Types
{
    public interface IReservationsApiClient
    {
        Task<ReservationValidationResult> ValidateReservation(ReservationValidationMessage request, CancellationToken cancellationToken);
        Task<ReservationAllocationStatusResult> GetReservationAllocationStatus(ReservationAllocationStatusMessage request, CancellationToken cancellationToken);
    }
}
