using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Types
{
    public enum PaymentStatus : short
    {
        //PendingApproval = 0, //TODO : Remove later
        Active = 1,
        Paused = 2,
        Withdrawn = 3,
        Completed = 4
    }
}
