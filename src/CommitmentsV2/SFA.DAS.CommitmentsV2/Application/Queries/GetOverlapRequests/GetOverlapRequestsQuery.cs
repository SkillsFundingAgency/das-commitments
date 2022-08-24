using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlapRequests
{
    public class GetOverlapRequestsQuery : IRequest<GetOverlapRequestsQueryResult>
    {
        public long DraftApprenticeshipId { get; }

        public GetOverlapRequestsQuery(long draftApprenticeshipId)
        {
            DraftApprenticeshipId = draftApprenticeshipId;
        }
    }
}
