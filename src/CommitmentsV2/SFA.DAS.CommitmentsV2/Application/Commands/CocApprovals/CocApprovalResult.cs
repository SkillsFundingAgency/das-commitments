using SFA.DAS.CommitmentsV2.Models;

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