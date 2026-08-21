namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

public class GetApprenticeshipApprovalQuery : IRequest<GetApprenticeshipApprovalQueryResult>
{
    public long ApprenticeshipId { get; }

    public Guid ApprovalRequestId { get; }

    public GetApprenticeshipApprovalQuery(long apprenticeshipId, Guid approvalRequestId)
    {
        ApprenticeshipId = apprenticeshipId;
        ApprovalRequestId = approvalRequestId;
    }
}