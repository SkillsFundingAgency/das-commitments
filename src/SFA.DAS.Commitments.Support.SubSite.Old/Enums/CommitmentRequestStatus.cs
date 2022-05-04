using System.ComponentModel;

namespace SFA.DAS.Commitments.Support.SubSite.Enums
{
    public enum RequestStatus
    {
        None, // No use here.

        [Description("New request")]
        NewRequest,

        [Description("Sent to provider")]
        SentToProvider,

        [Description("Sent for review")]
        SentForReview,

        [Description("Ready for review")]
        ReadyForReview,

        [Description("With Provider for approval")]
        WithProviderForApproval,

        [Description("Ready for approval")]
        ReadyForApproval,

        [Description("Approved")]
        Approved,

        //With sender for approval
        [Description("Pending")]
        WithSenderForApproval,

        //Rejected by transfer
        [Description("Rejected")]
        RejectedBySender
    }
}