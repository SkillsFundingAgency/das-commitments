namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class OverlapCheckResultOnStartDate
    {
        public OverlapCheckResultOnStartDate(bool hasOverlappingStartDate, long apprenticeshipId)
        {
            HasOverlappingStartDate = hasOverlappingStartDate;
            ApprenticeshipId = apprenticeshipId;
        }

        public bool HasOverlappingStartDate { get; }
        public long ApprenticeshipId { get; }

        public bool HasOverlaps => HasOverlappingStartDate;
    }
}