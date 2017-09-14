using System;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.UnitTests
{
    public interface IAcademicYearDateProvider
    {
        DateTime CurrentAcademicYearStartDate { get; }
        DateTime CurrentAcademicYearEndDate { get; }
        DateTime LastAcademicYearFundingPeriod { get; }
    }
}