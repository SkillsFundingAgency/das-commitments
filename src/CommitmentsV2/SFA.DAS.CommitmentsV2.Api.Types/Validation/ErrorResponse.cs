using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Api.Types.Validation
{
    [Serializable]
    public class ErrorResponse
    {
        public List<ErrorDetail> Errors { get; set;  }

        [JsonConstructor]
        public ErrorResponse(List<ErrorDetail> errors)
        {
            Errors = errors;
        }
    }
}