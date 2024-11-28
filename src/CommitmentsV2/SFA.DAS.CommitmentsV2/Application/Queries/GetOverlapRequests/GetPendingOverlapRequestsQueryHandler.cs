using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlapRequests;

public class GetPendingOverlapRequestsQueryHandler(Lazy<ProviderCommitmentsDbContext> db) : IRequestHandler<GetPendingOverlapRequestsQuery, GetPendingOverlapRequestsQueryResult>
{
    public async Task<GetPendingOverlapRequestsQueryResult> Handle(GetPendingOverlapRequestsQuery request, CancellationToken cancellationToken)
    {
        var result = await db.Value.OverlappingTrainingDateRequests
            .Where(p => p.DraftApprenticeshipId == request.DraftApprenticeshipId && p.Status == OverlappingTrainingDateRequestStatus.Pending)
            .Select(oltd => new GetPendingOverlapRequestsQueryResult(oltd.DraftApprenticeshipId, oltd.PreviousApprenticeshipId, oltd.CreatedOn))
            .SingleOrDefaultAsync(cancellationToken);
                

        return result;
    }
}