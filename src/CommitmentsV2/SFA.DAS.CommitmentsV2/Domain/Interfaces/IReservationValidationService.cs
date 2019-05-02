using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects.Reservations;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IReservationValidationService
    {
        Task<ReservationValidationResult> Validate(ReservationValidationRequest request, CancellationToken cancellationToken);
    }
}
