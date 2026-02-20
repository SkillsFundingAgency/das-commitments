namespace SFA.DAS.CommitmentsV2.Models;

//TODO: Status codes will be problematic for Devs because Pending values are different to one another, makes more sense to has Pending as 0
public enum CocApprovalItemStatus : byte
{
    AutoApproved = 1,
    AutoRejected = 2,
    Pending = 3,
    EmployerApproved = 4,
    EmployerRejected = 5
}
