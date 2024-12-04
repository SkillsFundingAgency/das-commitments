using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Reservations;

public class ValidationResultToReservationValidationErrorMapper : IOldMapper<ReservationValidationResult, Domain.Entities.Reservations.ReservationValidationResult>
{
    public Task<Domain.Entities.Reservations.ReservationValidationResult> Map(ReservationValidationResult source)
    {
        var errors = source.ValidationErrors.Select(sourceError =>
            new Domain.Entities.Reservations.ReservationValidationError(sourceError.PropertyName,
                sourceError.Reason)).ToArray();

        var result = new Domain.Entities.Reservations.ReservationValidationResult(errors);

        return Task.FromResult(result);
    }
}