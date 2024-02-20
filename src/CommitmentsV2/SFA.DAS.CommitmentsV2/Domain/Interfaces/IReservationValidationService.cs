using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IReservationValidationService
    {
        Task<Entities.Reservations.ReservationValidationResult> Validate(ReservationValidationRequest request, CancellationToken cancellationToken);
    }
}
