using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class DraftApprenticeshipDeletedEvent
    {
        public long DraftApprenticeshipId { get; set; }
        public long CohortId { get; set; }
        public string Uln { get; set; }
        public Guid? ReservationId { get; set; }
        public DateTime DeletedOn { get; set; }
    }
}
