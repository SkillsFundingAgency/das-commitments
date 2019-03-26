using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Domain.Exceptions
{
    public class DomainException : InvalidOperationException
    {
        public IEnumerable<DomainError> DomainErrors { get; }

        public DomainException(IEnumerable<DomainError> errors)
        {
            DomainErrors = errors;
        }
    }
}
