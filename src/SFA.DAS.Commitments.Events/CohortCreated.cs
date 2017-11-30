using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("cohort_created")]
    public class CohortCreated
    {
        //Needs a parameterless constructor to work with the message processing
        public CohortCreated()
        {
            
        }

        public CohortCreated(long accountId, long? providerId, long commitmentId)
        {
            AccountId = accountId;
            ProviderId = providerId;
            CommitmentId = commitmentId;
        }

        public long AccountId { get; private set; }
        public long? ProviderId { get; private set; }
        public long CommitmentId { get; private set; }
    }
}
