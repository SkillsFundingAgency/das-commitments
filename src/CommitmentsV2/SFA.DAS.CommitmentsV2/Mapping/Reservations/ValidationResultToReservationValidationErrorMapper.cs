using System.Linq;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects.Reservations;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Reservations
{
    public class ValidationResultToReservationValidationErrorMapper : IMapper<ValidationResult, ReservationValidationResult>
    {
        public ReservationValidationResult Map(ValidationResult source)
        {
            return new ReservationValidationResult(source.ValidationErrors.Select(sourceError =>
                    new ReservationValidationError(sourceError.PropertyName,
                        sourceError.Reason,
                        sourceError.Code))
                .ToArray());
        }
    }
}
