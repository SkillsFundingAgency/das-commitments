using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("approved_cohort_returned_to_provider")]
    public class ApprovedCohortReturnedToProvider
    {
        //Needs a parameterless constructor to work with the message processing
        public ApprovedCohortReturnedToProvider()
        {

        }

        public ApprovedCohortReturnedToProvider(long accountId, long providerId, long commitmentId)
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
