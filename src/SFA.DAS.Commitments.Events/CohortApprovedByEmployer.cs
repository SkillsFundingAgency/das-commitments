using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("cohort_approved_by_employer")]
    public class CohortApprovedByEmployer
    {
        //Needs a parameterless constructor to work with the message processing
        public CohortApprovedByEmployer()
        {
            
        }

        public CohortApprovedByEmployer(long accountId, long providerId, long commitmentId)
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
