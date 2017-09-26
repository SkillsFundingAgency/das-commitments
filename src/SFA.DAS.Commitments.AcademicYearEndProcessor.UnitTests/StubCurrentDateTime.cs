using System;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.UnitTests
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