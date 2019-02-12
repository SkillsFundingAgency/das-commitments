using System;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    public class CommitmentsApiException : Exception
    {
        public int ErrorCode;

        public CommitmentsApiException(int errorCode, string message) : this(errorCode, message, null)
        {
        }

        public CommitmentsApiException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
