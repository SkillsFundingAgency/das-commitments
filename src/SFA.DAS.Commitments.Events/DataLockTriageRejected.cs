using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("data_lock_triage_rejected")]
    public class DataLockTriageRejected
    {
        //Needs a parameterless constructor to work with the message processing
        public DataLockTriageRejected()
        {

        }

        public DataLockTriageRejected(long accountId, long providerId, long apprenticeshipId)
        {
            AccountId = accountId;
            ProviderId = providerId;
            ApprenticeshipId = apprenticeshipId;
        }

        public long AccountId { get; set; }
        public long ProviderId { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
