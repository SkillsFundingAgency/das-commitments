namespace SFA.DAS.CommitmentsV2.Shared.Interfaces
{
    public interface IAcademicYearDateProvider
    {
        DateTime CurrentAcademicYearStartDate { get; }
        DateTime CurrentAcademicYearEndDate { get; }

        DateTime LastAcademicYearFundingPeriod { get; }
    }
}
