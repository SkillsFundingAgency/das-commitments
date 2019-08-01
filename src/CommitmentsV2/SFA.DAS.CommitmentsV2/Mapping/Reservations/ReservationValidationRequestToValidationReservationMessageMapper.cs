using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Reservations
{
    public class ReservationValidationRequestToValidationReservationMessageMapper : IMapper<ReservationValidationRequest, ValidationReservationMessage>
    {
        public Task<ValidationReservationMessage> Map(ReservationValidationRequest source)
        {
            return Task.FromResult(new ValidationReservationMessage
            {
                ReservationId = source.ReservationId,
                StartDate = source.StartDate,
                CourseCode = source.CourseCode
            });
        }
    }
}
