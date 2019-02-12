using System;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    [Serializable]
    public class ErrorDetail
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
