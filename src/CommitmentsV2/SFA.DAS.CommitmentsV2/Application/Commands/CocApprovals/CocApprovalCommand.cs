namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class CocApprovalCommand : IRequest<CocApprovalResult>
{
    public CocApprovalDetails CocApprovalDetails { get; set; }
    public AggregrationAction Action { get; set; }
    public Guid? PreviousApprovalRequestId { get; set; }
}

public enum AggregrationAction
{
    CreateNew,
    SupersedePrevious,
    CancelPrevious
}