using System;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.UnitTests
{
    public class InvalidAcademicYearException : Exception
    {
        public InvalidAcademicYearException(string message): base(message)
        {
            
        }
    }
}