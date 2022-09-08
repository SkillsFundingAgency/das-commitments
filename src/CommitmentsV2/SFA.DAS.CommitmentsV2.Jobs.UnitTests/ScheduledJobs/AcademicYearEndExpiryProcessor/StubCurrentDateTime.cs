using System;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.UnitTests
{
    public class StubCurrentDateTime : ICurrentDateTime
    {
        public StubCurrentDateTime(DateTime now)
        {
            UtcNow = now;
        }
        public DateTime UtcNow { get; }
    }
}