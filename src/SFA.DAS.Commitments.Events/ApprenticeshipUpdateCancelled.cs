using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("apprenticeship_update_cancelled")]
    public class ApprenticeshipUpdateCancelled
    {
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
