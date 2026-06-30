namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class OverlapCheckResultOnStartDate
{
    public OverlapCheckResultOnStartDate(bool hasOverlappingStartDate, long? apprenticeshipId, bool hasOverlapWithIlrWithdrawnApprenticeship)
    {
        HasOverlappingStartDate = hasOverlappingStartDate;
        ApprenticeshipId = apprenticeshipId;
        HasOverlapWithIlrWithdrawnApprenticeship = hasOverlapWithIlrWithdrawnApprenticeship;
    }

    public bool HasOverlappingStartDate { get; }
    public long? ApprenticeshipId { get; }
    public bool HasOverlapWithIlrWithdrawnApprenticeship { get; }
}