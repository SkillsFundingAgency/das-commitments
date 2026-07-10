using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class UlnUtilisation
{
    public UlnUtilisation(long apprenticeshipId, string uln, DateTime startDate, DateTime endDate, bool isWithdrawn)
    {
        ApprenticeshipId = apprenticeshipId;
        Uln = uln;
        DateRange = new CourseDateRange(startDate, endDate, isWithdrawn);
    }

    public long ApprenticeshipId { get; }
    public string Uln { get; }
    public CourseDateRange DateRange { get; }
}