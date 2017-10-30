using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("cohort_approved_by_employer")]
    public class CohortApprovedByEmployer
    {
        public CohortApprovedByEmployer(long accountId, long providerId, long commitmentId)
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
