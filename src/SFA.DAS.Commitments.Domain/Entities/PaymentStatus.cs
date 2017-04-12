using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public enum PaymentStatus
    {
        PendingApproval = 0, 
        Active = 1,                 // aka "on programme", will be paid
        Paused = 2,                 // temporarily stopping payment, will not be paid
        Withdrawn = 3,              // permanently stopped payment, will not be paid
        Completed = 4,              // training has been completed, will not be paid
        Deleted = 5
    }
}
