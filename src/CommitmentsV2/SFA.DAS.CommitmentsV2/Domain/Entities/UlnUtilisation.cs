using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class UlnUtilisation
{
    public UlnUtilisation(long apprenticeshipId, string uln, DateTime startDate, DateTime endDate)
    {
        ApprenticeshipId = apprenticeshipId;
        Uln = uln;
        DateRange = new DateRange(startDate, endDate);
    }

    public long ApprenticeshipId { get; }
    public string Uln { get; }
    public DateRange DateRange { get; }
    public DateTime? ActualStartDate { get; set; }
    public LearningType? LearningType { get; set; }
}