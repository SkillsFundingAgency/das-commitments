using System;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    [Serializable]
    public class ErrorDetails
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
