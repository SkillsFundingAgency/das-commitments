using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Shared.Services;

public class AcademicYearDateProvider(ICurrentDateTime currentDateTime) : IAcademicYearDateProvider
{
    public DateTime CurrentAcademicYearStartDate
    {
        get
        {
            var now = currentDateTime.UtcNow;
            var cutoffUtc = new DateTime(now.Year, 8, 1, 0, 0, 0, DateTimeKind.Utc);
            return now >= cutoffUtc ? cutoffUtc : new DateTime(now.Year - 1, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    public DateTime CurrentAcademicYearEndDate => CurrentAcademicYearStartDate.AddYears(1).AddDays(-1);

    public DateTime LastAcademicYearFundingPeriod => new DateTime(CurrentAcademicYearStartDate.Year, 10, 19, 18, 0, 0, DateTimeKind.Utc);
}