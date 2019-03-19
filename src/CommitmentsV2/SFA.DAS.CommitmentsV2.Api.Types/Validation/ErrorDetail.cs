using System;
using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Api.Types.Validation
{
    [Serializable]
    public class ErrorDetail
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ErrorCode { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        public string Message { get; }

        public ErrorDetail(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
        public ErrorDetail(int errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }

        [JsonConstructor]
        protected ErrorDetail(int? errorCode, string field, string message)
        {
            ErrorCode = errorCode;
            Field = field;
            Message = message;
        }
    }
}
