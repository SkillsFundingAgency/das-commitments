using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("provider_cohort_approval_undone_by_employer_update")]
    public class ProviderCohortApprovalUndoneByEmployerUpdate
    {
        //Needs a parameterless constructor to work with the message processing
        public ProviderCohortApprovalUndoneByEmployerUpdate()
        {
            
        }

        public ProviderCohortApprovalUndoneByEmployerUpdate(long accountId, long providerId, long commitmentId)
        {
            AccountId = accountId;
            ProviderId = providerId;
            CommitmentId = commitmentId;
        }

        public long CommitmentId { get; set; }

        public long ProviderId { get; set; }

        public long AccountId { get; set; }
    }
}
