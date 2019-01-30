using System;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    public class ApiException : Exception
    {
        public int Errorcode;

        public ApiException(int errorCode, string message) : this(errorCode, message, null)
        {
        }

        public ApiException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            Errorcode = errorCode;
        }
    }
}
