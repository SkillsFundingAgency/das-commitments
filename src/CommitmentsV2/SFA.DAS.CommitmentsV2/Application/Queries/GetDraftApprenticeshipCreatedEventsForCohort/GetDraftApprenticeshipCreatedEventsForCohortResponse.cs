using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort
{
    public class GetDraftApprenticeshipCreatedEventsForCohortResponse
    {
        public IEnumerable<DraftApprenticeshipCreatedEvent> DraftApprenticeshipCreatedEvents { get; }

        public GetDraftApprenticeshipCreatedEventsForCohortResponse(IEnumerable<DraftApprenticeshipCreatedEvent> events)
        {
            DraftApprenticeshipCreatedEvents = events;
        }
    }
}
