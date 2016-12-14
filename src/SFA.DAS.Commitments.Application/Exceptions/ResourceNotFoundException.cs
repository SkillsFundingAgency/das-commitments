using System;

namespace SFA.DAS.Commitments.Application.Exceptions
{
    public sealed class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException() : base() {}

        public ResourceNotFoundException(string message) : base(message) {}
    }
}
