using System;

namespace SFA.DAS.CommitmentsV2.Domain.Exceptions
{
    [Serializable]
    public class RepositoryConstraintException : Exception
    {
        public RepositoryConstraintException() { }

        public RepositoryConstraintException(string message) : base(message) { }

        public RepositoryConstraintException(string message, Exception inner) : base(message, inner) { }

        protected RepositoryConstraintException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
