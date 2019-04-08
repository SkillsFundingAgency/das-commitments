using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Domain.Exceptions
{
    public static class DomainErrorExtensions
    {
        public static void ThrowIfAny(this DomainError[] errors)
        {
            if (errors.Length > 0)
            {
                throw new DomainException(errors);
            }
        }

        public static void ThrowIfAny(this List<DomainError> errors)
        {
            if (errors.Count > 0)
            {
                throw new DomainException(errors);
            }
        }
    }
}
