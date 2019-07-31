using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types.Types;

namespace SFA.DAS.Reservations.Api.Types
{
    public interface IReservationsApiClient
    {
        Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, CancellationToken cancellationToken);
    }
}
