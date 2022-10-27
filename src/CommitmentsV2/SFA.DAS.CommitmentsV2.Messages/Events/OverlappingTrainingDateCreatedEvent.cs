namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class OverlappingTrainingDateCreatedEvent
    {
        public long ApprenticeshipId { get; }
        public string Uln { get; set; }

        public OverlappingTrainingDateCreatedEvent(long apprenticeshipId, string uln)
        {
            ApprenticeshipId = apprenticeshipId;
            Uln = uln;
        }
    }
}
