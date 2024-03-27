using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort
{
    public class GetDraftApprenticeshipCreatedEventsForCohortQueryResult
    {
        public DraftApprenticeshipCreatedEvent[] DraftApprenticeshipCreatedEvents { get; }

        public GetDraftApprenticeshipCreatedEventsForCohortQueryResult(IEnumerable<DraftApprenticeshipCreatedEvent> events)
        {
            DraftApprenticeshipCreatedEvents = events.ToArray();
        }
    }
}
