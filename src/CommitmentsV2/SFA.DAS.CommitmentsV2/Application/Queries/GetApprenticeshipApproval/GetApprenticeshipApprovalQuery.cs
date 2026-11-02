namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

public class GetApprenticeshipApprovalQuery : IRequest<GetApprenticeshipApprovalQueryResult>
{
    public long ApprenticeshipId { get; }

    public GetApprenticeshipApprovalQuery(long apprenticeshipId)
    {
        ApprenticeshipId = apprenticeshipId;
    }
}
