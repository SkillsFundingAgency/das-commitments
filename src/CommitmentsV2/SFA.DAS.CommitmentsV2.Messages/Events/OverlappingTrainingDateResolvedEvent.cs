namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class OverlappingTrainingDateResolvedEvent
    {
        public long ApprenticeshipId { get;set; }
        public long CohortId { get; set; }
        public OverlappingTrainingDateResolvedEvent(long apprenticeshipId,long cohortId)
        {
            ApprenticeshipId = apprenticeshipId;
            CohortId = cohortId;
        }
    }
}