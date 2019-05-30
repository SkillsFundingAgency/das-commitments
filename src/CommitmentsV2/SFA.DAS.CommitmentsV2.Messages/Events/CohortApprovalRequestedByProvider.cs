namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortApprovalRequestedByProvider
    {
        public long AccountId { get; set; }
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
    }
}
