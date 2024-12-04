namespace SFA.DAS.CommitmentsV2.Domain.Entities;

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
            return "This email address is in use on another apprentice record. You need to enter a different email address.";
        }

        return "You need to enter a unique email address for each apprentice.";
    }
}