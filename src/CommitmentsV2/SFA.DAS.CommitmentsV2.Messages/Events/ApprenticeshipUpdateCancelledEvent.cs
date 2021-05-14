namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipUpdateCancelledEvent
    {
        public long AccountId { get; set; }
        public long ProviderId { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
