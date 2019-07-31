using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.Reservations
{
    public class ValidationResultToReservationValidationErrorMapper : IMapper<DAS.Reservations.Api.Types.Types.ReservationValidationResult, Domain.Entities.Reservations.ReservationValidationResult>
    {
        public Task<Domain.Entities.Reservations.ReservationValidationResult> Map(DAS.Reservations.Api.Types.Types.ReservationValidationResult source)
        {
            var errors = source.ValidationErrors.Select(sourceError =>
                    new Domain.Entities.Reservations.ReservationValidationError(sourceError.PropertyName,
                        sourceError.Reason)).ToArray();

            var result = new Domain.Entities.Reservations.ReservationValidationResult(errors);

            return Task.FromResult(result);
        }
    }
}
