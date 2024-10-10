namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries;

public class GetDataLockSummariesQuery : IRequest<GetDataLockSummariesQueryResult>
{
    public long ApprenticeshipId { get; }

    public GetDataLockSummariesQuery(long apprenticeshipId)
    {
        ApprenticeshipId = apprenticeshipId;
    }
}