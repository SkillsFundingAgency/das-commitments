namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class CocApprovalResponse
{
    public List<CocApprovalResult> Items { get; set; } = new();
}

public class CocApprovalResult
{
    public string ChangeType { get; set; }
    public CocData Data { get; set; }
}

public enum CocApprovalResultStatus
{
    Success,
    Failed
}