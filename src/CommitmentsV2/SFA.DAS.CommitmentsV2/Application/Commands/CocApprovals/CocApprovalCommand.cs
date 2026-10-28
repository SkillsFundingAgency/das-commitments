using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class CocApprovalCommand : IRequest<CocApprovalResult>
{
    public CocApprovalDetails CocApprovalDetails { get; set; }
    public Guid LearningKey { get; set; }
    public AggregationAction Action { get; set; }
    public List<PriceRecord> Prices { get; set; }
    public List<string> ReversedFields { get; set; } = [];
}

public enum AggregationAction
{
    CreateNew,
    SupersedePrevious,
    CancelPrevious
}