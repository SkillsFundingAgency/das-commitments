using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Api.Types.Validation
{
    public class ReservationValidationResult
    {
        public ReservationValidationResult(IEnumerable<ReservationValidationError> errors)
        {
            Errors = errors.ToArray();
            HasErrors = Errors.Length > 0;
        }

        public bool HasErrors { get; set; }
        public ReservationValidationError[] Errors { get; set; }
    }
}