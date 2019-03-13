using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Api.Types.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    [Serializable]
    public class ErrorResponse
    {
        public ErrorType ErrorType { get; set; }

        public IEnumerable<ErrorDetail> ErrorDetails { get; set; }
    }

}
