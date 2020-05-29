namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ChangeOfPartyRequestCohortCreatedEvent
    {
        public long CohortId { get; set; }

        public ChangeOfPartyRequestCohortCreatedEvent(long cohortId)
        {
            CohortId = cohortId;
        }
    }
}
