namespace SFA.DAS.CommitmentsV2.Events
{
    public class ApprenticeshipUpdateCancelled
    {
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
