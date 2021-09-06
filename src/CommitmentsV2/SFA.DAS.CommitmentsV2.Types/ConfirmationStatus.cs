using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Types
{
    public enum ConfirmationStatus : short
    {
        [Description("N/A")]
        NA = 4,
        Overdue = 3,
        Unconfirmed = 2,
        Confirmed = 1
    }
}