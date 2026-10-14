namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovalRequest;

public class GetApprovalRequestQuery : IRequest<GetApprovalRequestQueryResult>
{
    public long ApprenticeshipId { get; set; }
    public byte CocApprovalItemStatus { get; set; }
    public long AccountId { get; set; }
}