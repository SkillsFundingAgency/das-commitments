using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public interface IApprenticeshipDeletedEvent
    {
        long Apprenticeship { get; set; }
        long CommitmentId { get; set; }
        string Uln { get; set; }
        Guid? ReservationId { get; set; }
        DateTime? CourseStartDate { get; set; }
        string CourseCode { get; set; }
        DateTime DeletedOn { get; set; }
    }
}
