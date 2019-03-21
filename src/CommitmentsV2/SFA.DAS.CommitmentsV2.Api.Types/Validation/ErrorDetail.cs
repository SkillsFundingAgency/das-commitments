using System;
using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Api.Types.Validation
{
    [Serializable]
    public class ErrorDetail
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        public string Message { get; }

        [JsonConstructor]
        public ErrorDetail(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
    }
}
