using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    [Serializable]
    public class ErrorResponse
    {
        public ErrorType ErrorType { get; set; }

        public IEnumerable<ErrorDetail> ErrorDetails { get; set; }
    }

}
