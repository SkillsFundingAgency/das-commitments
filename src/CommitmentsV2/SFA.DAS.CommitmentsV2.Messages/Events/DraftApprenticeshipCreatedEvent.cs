using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class DraftApprenticeshipCreatedEvent
    {
        public long DraftApprenticeshipId { get; }
        public long CohortId { get; }
        public string Uln { get; }
        public Guid ReservationId { get; }
        public DateTime CreatedOn { get; }

        public DraftApprenticeshipCreatedEvent(long draftApprenticeshipId, long cohortId, string uln, Guid reservationId, DateTime createdOn)
        {
            DraftApprenticeshipId = draftApprenticeshipId;
            CohortId = cohortId;
            Uln = uln;
            ReservationId = reservationId;
            CreatedOn = createdOn;
        }
    }
}