namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class PostCocApprovalCommand() : IRequest<CocApprovalResult>
{
    public CocApprovalDetails CocApprovalDetails { get; set; }
}
