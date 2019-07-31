using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Reservations.Api.Types.Types;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    public interface IReservationValidationService
    {
        /// <summary>
        ///     Invokes reservations API if the apprenticeship has a reservation ID and returns the validation result.
        /// </summary>
        Task<ReservationValidationResult> CheckReservation(ReservationValidationServiceRequest request);
    }
}