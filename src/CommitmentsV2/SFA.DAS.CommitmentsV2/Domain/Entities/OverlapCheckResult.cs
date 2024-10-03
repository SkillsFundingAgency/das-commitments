namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class OverlapCheckResult
{
    public OverlapCheckResult(bool hasOverlappingStartDate, bool hasOverlappingEndDate)
    {
        HasOverlappingStartDate = hasOverlappingStartDate;
        HasOverlappingEndDate = hasOverlappingEndDate;
    }

    public bool HasOverlappingStartDate { get; }
    public bool HasOverlappingEndDate { get; }

    public bool HasOverlaps => HasOverlappingStartDate || HasOverlappingEndDate;
}