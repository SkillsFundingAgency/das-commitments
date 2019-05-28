using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class DraftApprenticeshipUpdatedEvent
    {
        public long DraftApprenticeshipId { get; }
        public long CohortId { get; }
        public string Uln { get; }
        public Guid? ReservationId { get; }
        public DateTime UpdatedOn { get; }

        public DraftApprenticeshipUpdatedEvent(long draftApprenticeshipId, long cohortId, string uln, Guid? reservationId, DateTime updatedOn)
        {
            DraftApprenticeshipId = draftApprenticeshipId;
            CohortId = cohortId;
            Uln = uln;
            ReservationId = reservationId;
            UpdatedOn = updatedOn;
        }
    }
}