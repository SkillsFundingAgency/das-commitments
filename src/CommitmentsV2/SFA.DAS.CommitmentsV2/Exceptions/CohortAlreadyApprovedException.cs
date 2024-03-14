using System;
using System.Runtime.Serialization;

namespace SFA.DAS.CommitmentsV2.Exceptions
{
    public class CohortAlreadyApprovedException : Exception
    {

        public CohortAlreadyApprovedException()
        {
        }

        public CohortAlreadyApprovedException(string message) : base(message)
        {
        }

        protected CohortAlreadyApprovedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
