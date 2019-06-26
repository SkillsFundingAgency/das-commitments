using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Reservations
{
    public class ValidationResultToReservationValidationErrorMapper : IMapper<ValidationResult, ReservationValidationResult>
    {
        public Task<ReservationValidationResult> Map(ValidationResult source)
        {
            return Task.FromResult(new ReservationValidationResult(source.ValidationErrors.Select(sourceError =>
                    new ReservationValidationError(sourceError.PropertyName, sourceError.Reason))
                .ToArray()));
        }
    }
}
