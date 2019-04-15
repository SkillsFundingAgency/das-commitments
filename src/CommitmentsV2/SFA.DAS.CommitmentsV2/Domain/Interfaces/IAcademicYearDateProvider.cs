using System;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IAcademicYearDateProvider
    {
        DateTime CurrentAcademicYearStartDate { get; }
        DateTime CurrentAcademicYearEndDate { get; }

        DateTime LastAcademicYearFundingPeriod { get; }
    }
}
