using System;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.Updater
{
    public class InvalidAcademicYearException : Exception
    {
        public InvalidAcademicYearException(string message): base(message)
        {
            
        }
    }
}