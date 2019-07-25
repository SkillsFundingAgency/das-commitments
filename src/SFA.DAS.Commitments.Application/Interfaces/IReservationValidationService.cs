using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange;
using SFA.DAS.Commitments.Application.Services;

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