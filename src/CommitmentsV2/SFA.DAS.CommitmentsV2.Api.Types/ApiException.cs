using System;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    public class ApiException : Exception
    {
        public int ErrorCode;

        public ApiException(int errorCode, string message) : this(errorCode, message, null)
        {
        }

        public ApiException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
