using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Domain.Exceptions
{
    public static class DomainErrorExtensions
    {
        public static void ThrowIfAny(this IEnumerable<DomainError> errors)
        {
            var domainErrors = errors.ToList();

            if (domainErrors.Any())
            {
                throw new DomainException(domainErrors);
            }
        }
    }
}
