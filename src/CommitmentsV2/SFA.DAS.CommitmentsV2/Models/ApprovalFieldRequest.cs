namespace SFA.DAS.CommitmentsV2.Models;

public class ApprovalFieldRequest
{
    public Guid Id { get; set; }
    public DateTime Created { get; }
    public DateTime? Updated { get; set; }
    public string Field { get; set; }
    public string Old { get; set; }
    public string New { get; set; }
    public Guid ApprovalRequestId { get; set; }
    public ApprovalRequest ApprovalRequest { get; set; }
    public CocApprovalItemStatus? Status { get; set; }
    public string ApproverId { get; set; }
    public string Reason { get; set; }
}
