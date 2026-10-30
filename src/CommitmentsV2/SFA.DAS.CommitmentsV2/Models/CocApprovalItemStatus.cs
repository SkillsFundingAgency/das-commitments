namespace SFA.DAS.CommitmentsV2.Models;

public enum CocApprovalItemStatus : byte
{
    AutoApproved = 1,
    AutoRejected = 2,
    Pending = 3,
    EmployerApproved = 4,
    EmployerRejected = 5,
    Superseded = 6,
    Reversal = 7,
}
