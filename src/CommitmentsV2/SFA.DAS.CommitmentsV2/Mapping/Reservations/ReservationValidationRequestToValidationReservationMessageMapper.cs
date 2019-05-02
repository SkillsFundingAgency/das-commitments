using SFA.DAS.CommitmentsV2.Domain.ValueObjects.Reservations;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Reservations
{
    public class ReservationValidationRequestToValidationReservationMessageMapper : IMapper<ReservationValidationRequest, ValidationReservationMessage>
    {
        public ValidationReservationMessage Map(ReservationValidationRequest source)
        {
            return new ValidationReservationMessage
            {
                AccountId = source.AccountId,
                ReservationId = source.ReservationId,
                StartDate = source.StartDate,
                ProviderId = source.ProviderId,
                AccountLegalEntityPublicHashedId = source.AccountLegalEntityPublicHashedId,
                CourseCode = source.CourseCode
            };
        }
    }
}
