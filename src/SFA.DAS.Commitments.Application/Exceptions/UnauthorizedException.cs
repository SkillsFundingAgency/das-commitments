using System;

namespace SFA.DAS.Commitments.Application.Exceptions
{
    public sealed class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base() {}

        public UnauthorizedException(string message) : base(message) {}
    }
}
