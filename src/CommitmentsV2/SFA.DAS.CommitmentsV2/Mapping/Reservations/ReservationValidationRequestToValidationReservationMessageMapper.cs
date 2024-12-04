using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Reservations;

public class ReservationValidationRequestToValidationReservationMessageMapper : IOldMapper<ReservationValidationRequest, ReservationValidationMessage>
{
    public Task<ReservationValidationMessage> Map(ReservationValidationRequest source)
    {
        return Task.FromResult(new ReservationValidationMessage
        {
            ReservationId = source.ReservationId,
            StartDate = source.StartDate,
            CourseCode = source.CourseCode
        });
    }
}