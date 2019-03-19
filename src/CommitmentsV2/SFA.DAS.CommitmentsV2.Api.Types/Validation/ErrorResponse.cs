using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SFA.DAS.CommitmentsV2.Api.Types.Validation
{
    [Serializable]
    public class ErrorResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType ErrorType { get; set; }

        public List<ErrorDetail> Errors { get; set;  }

        public ErrorResponse(ErrorType errorType, int errorCode, string message)
        {
            ErrorType = errorType;
            Errors = new List<ErrorDetail> { new ErrorDetail(errorCode, message) };
        }

        [JsonConstructor]
        public ErrorResponse([JsonConverter(typeof(StringEnumConverter))]ErrorType errorType, List<ErrorDetail> errors)
        {
            ErrorType = errorType;
            Errors = errors;
        }
    }
}