using System;

namespace SFA.DAS.Commitments.Application.Exceptions
{
    public sealed class InvalidRequestException : Exception
    {
        public InvalidRequestException() : base("Request is invalid")
        {
        }
    }
}
