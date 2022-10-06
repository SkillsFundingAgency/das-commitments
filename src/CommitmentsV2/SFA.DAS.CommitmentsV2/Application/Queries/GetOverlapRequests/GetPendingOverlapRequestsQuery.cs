using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlapRequests
{
    public class GetPendingOverlapRequestsQuery : IRequest<GetPendingOverlapRequestsQueryResult>
    {
        public long DraftApprenticeshipId { get; }

        public GetPendingOverlapRequestsQuery(long draftApprenticeshipId)
        {
            DraftApprenticeshipId = draftApprenticeshipId;
        }
    }
}
