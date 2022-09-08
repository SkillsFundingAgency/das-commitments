//using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.CommitmentsV2.Events
{
    //[MessageGroup("apprenticeship_update_cancelled")]
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

        public long AccountId { get; set; }
        public long ProviderId { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
