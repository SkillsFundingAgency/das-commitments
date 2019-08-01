using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationValidationResult
    {
        public ReservationValidationResult(IEnumerable<ReservationValidationError> errors)
        {
            ValidationErrors = errors.ToArray();
        }

        public ReservationValidationResult()
        {
            ValidationErrors = new ReservationValidationError[0];
        }

        public bool HasErrors => ValidationErrors.Length > 0;
        public bool IsOkay => !HasErrors;
        public ReservationValidationError[] ValidationErrors { get; set; }
    }
}