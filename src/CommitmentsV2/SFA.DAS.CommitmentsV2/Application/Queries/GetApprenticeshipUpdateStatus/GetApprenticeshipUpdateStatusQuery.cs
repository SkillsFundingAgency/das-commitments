namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdateStatus;

public class GetApprenticeshipUpdateStatusQuery : IRequest<GetApprenticeshipUpdateStatusQueryResult>
{
    public long ApprenticeshipId { get; }

    public GetApprenticeshipUpdateStatusQuery(long apprenticeshipId)
    {
        ApprenticeshipId = apprenticeshipId;
    }
}