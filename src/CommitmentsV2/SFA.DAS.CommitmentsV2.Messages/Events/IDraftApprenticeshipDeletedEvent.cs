using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public interface IDraftApprenticeshipDeletedEvent
    {
        long DraftApprenticeshipId { get; set; }
        long CohortId { get; set; }
        string Uln { get; set; }
        Guid? ReservationId { get; set; }
        DateTime DeletedOn { get; set; }
    }
}
