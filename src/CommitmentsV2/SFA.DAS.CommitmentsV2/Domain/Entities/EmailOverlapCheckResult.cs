namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class EmailOverlapCheckResult
    {
        public long RowId { get; }
        public OverlapStatus OverlapStatus { get; }
        public bool FoundOnFullyApprovedApprenticeship { get; }

        public EmailOverlapCheckResult(long rowId, OverlapStatus overlapStatus, bool foundOnFullyApprovedApprenticeship)
        {
            RowId = rowId;
            OverlapStatus = overlapStatus;
            FoundOnFullyApprovedApprenticeship = foundOnFullyApprovedApprenticeship;
        }

        public string BuildErrorMessage()
        {
            if (FoundOnFullyApprovedApprenticeship)
            {
                return "You need to enter a unique email address.";
            }

            return "You need to enter a unique email address for each apprentice.";
        }
    }
}