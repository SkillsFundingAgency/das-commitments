using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovalRequest;

public class GetApprovalRequestQueryResult
{
    public string ApprenticeName { get; set; }
    public List<ApprovalRequestItem> ApprovalRequests { get; set; }
}