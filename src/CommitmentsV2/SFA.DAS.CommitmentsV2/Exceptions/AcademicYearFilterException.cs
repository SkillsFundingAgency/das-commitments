using System;
using System.Runtime.Serialization;

namespace SFA.DAS.CommitmentsV2.Exceptions
{
    public class AcademicYearFilterException : Exception
    {
        public AcademicYearFilterException()
        {
        }

        public AcademicYearFilterException(string message) : base(message)
        {
        }

        public AcademicYearFilterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AcademicYearFilterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}