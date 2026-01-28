namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class CocApprovalResult
{
    public CocApprovalResultStatus Status { get; set; }
    public List<CocUpdateResult> Items { get; set; }
}

public class CocUpdateResult
{
    public CocChangeField Field { get; set; }
    public CocApprovalItemStatus Status { get; set; }
    public string Reason { get; set; }

}



public class CocApprovalItemResult
{
    public string ChangeType { get; set; }
    public string Status { get; set; }
    public string Reason { get; set; }
}

//TODO: Status codes will be problematic for Devs because Pending values are different to one another, makes more sense to has Pending as 0
public enum CocApprovalItemStatus : byte
{
    AutoApproved = 1,
    AutoRejected = 2,
    Pending = 3,
    EmployerApproved = 4,
    EmployerRejected = 5
}

public enum CocApprovalResultStatus : byte
{
    Pending = 1,
    Complete = 2,
    Superseded = 3,
    Cancelled = 4
}