using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("apprenticeship_update_cancelled")]
    public class ApprenticeshipUpdateCancelled
    {

        //Needs a parameterless constructor to work with the message processing
        public ApprenticeshipUpdateCancelled()
        {
            
        }

        public ApprenticeshipUpdateCancelled(long accountId, long providerId, long apprenticeshipId)
        {
            AccountId = accountId;
            ProviderId = providerId;
            ApprenticeshipId = apprenticeshipId;
        }

        public long AccountId { get; private set; }
        public long ProviderId { get; private set; }
        public long ApprenticeshipId { get; private set; }
    }
}
