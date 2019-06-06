using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Reservations
{
    public class ReservationValidationRequestToValidationReservationMessageMapper : IMapper<ReservationValidationRequest, ValidationReservationMessage>
    {
        public Task<ValidationReservationMessage> Map(ReservationValidationRequest source)
        {
            return Task.FromResult(new ValidationReservationMessage
            {
                AccountId = source.AccountId,
                ReservationId = source.ReservationId,
                StartDate = source.StartDate,
                ProviderId = source.ProviderId,
                AccountLegalEntityPublicHashedId = source.AccountLegalEntityPublicHashedId,
                CourseCode = source.CourseCode
            });
        }
    }
}
