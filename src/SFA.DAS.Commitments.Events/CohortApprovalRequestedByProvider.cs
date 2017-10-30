using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("cohort_approval_requested_by_provider")]
    public class CohortApprovalRequestedByProvider
    {
        public CohortApprovalRequestedByProvider(long accountId, long providerId, long commitmentId)
        {
            AccountId = accountId;
            ProviderId = providerId;
            CommitmentId = commitmentId;
        }

        public long AccountId { get; private set; }
        public long ProviderId { get; private set; }
        public long CommitmentId { get; private set; }
    }
}
