namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;

public class GetChangeOfPartyRequestsQuery : IRequest<GetChangeOfPartyRequestsQueryResult>
{
    public long ApprenticeshipId { get; }

    public GetChangeOfPartyRequestsQuery(long apprenticeshipId)
    {
        ApprenticeshipId = apprenticeshipId;
    }
}