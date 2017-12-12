using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("cohort_approval_requested_by_provider")]
    public class CohortApprovalRequestedByProvider
    {
        //Needs a parameterless constructor to work with the message processing
        public CohortApprovalRequestedByProvider()
        {
            
        }

        public CohortApprovalRequestedByProvider(long accountId, long providerId, long commitmentId)
        {
            AccountId = accountId;
            ProviderId = providerId;
            CommitmentId = commitmentId;
        }

        public long AccountId { get; set; }
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
    }
}
