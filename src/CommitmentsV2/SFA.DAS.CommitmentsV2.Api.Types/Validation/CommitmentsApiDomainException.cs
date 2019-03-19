using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Validation
{
    public class CommitmentsApiDomainException : Exception
    {
        public int ErrorCode;

        public CommitmentsApiDomainException(int errorCode, string message) : this(errorCode, message, null)
        {
        }

        public CommitmentsApiDomainException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
