using System;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.UnitTests
{
    public class StubCurrentDateTime: ICurrentDateTime
    {
        public StubCurrentDateTime(DateTime now)
        {
            Now = now;
        }
        public DateTime Now { get; }
    }
}