using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Domain.Exceptions
{
    public static class DomainErrorExtensions
    {
        public static void ThrowIfAny(this List<DomainError> errors)
        {
            if (errors.Any())
            {
                throw new DomainException(errors);
            }
        }

        public static void ThrowIfAny(this List<BulkUploadValidationError> errors)
        {
            if (errors.Any())
            {
                throw new BulkUploadDomainException(errors);
            }
        }
    }
}
