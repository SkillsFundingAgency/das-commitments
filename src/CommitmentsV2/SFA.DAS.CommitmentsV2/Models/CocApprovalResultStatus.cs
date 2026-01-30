namespace SFA.DAS.CommitmentsV2.Models;

public enum CocApprovalResultStatus : byte
{
    Pending = 1,
    Complete = 2,
    Superseded = 3,
    Cancelled = 4
}