namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class OverlapCheckResult
    {
        public OverlapCheckResult(bool overlappingStartDate, bool overlappingEndDate)
        {
            OverlappingStartDate = overlappingStartDate;
            OverlappingEndDate = overlappingEndDate;
        }

        public bool OverlappingStartDate { get; private set; }
        public bool OverlappingEndDate { get; private set; }

        public bool HasOverlaps => OverlappingStartDate || OverlappingEndDate;
    }
}
