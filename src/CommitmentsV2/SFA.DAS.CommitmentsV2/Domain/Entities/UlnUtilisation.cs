using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class UlnUtilisation
{
    public UlnUtilisation(long apprenticeshipId, string uln, DateTime startDate, DateTime endDate, bool isActive)
    {
        ApprenticeshipId = apprenticeshipId;
        Uln = uln;
        DateRange = new CourseDateRange(startDate, endDate, isActive);
    }

    public long ApprenticeshipId { get; }
    public string Uln { get; }
    public CourseDateRange DateRange { get; }
}