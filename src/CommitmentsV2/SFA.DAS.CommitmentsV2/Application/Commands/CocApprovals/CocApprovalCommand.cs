using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class CocApprovalCommand : IRequest<CocApprovalResult>
{
    public CocApprovalDetails CocApprovalDetails { get; set; }
    public Guid LearningKey { get; set; }
    public AggregrationAction Action { get; set; }
}

public enum AggregrationAction
{
    CreateNew,
    SupersedePrevious,
    CancelPrevious
}