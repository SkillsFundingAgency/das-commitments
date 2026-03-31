namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class PutCocApprovalCommand() : IRequest<CocApprovalResult>
{
    public CocApprovalDetails CocApprovalCommand { get; set; }
}