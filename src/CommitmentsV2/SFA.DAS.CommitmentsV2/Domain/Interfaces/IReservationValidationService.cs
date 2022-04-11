using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IReservationValidationService
    {
        Task<Entities.Reservations.ReservationValidationResult> Validate(ReservationValidationRequest request, CancellationToken cancellationToken);
        Task<BulkValidationResults> BulkValidate(IEnumerable<ReservationRequest> request, CancellationToken cancellationToken);
    }
}
