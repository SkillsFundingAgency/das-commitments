using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlapRequests
{
    public class GetPendingOverlapRequestsQueryHandler : IRequestHandler<GetPendingOverlapRequestsQuery, GetPendingOverlapRequestsQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetPendingOverlapRequestsQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetPendingOverlapRequestsQueryResult> Handle(GetPendingOverlapRequestsQuery request, CancellationToken cancellationToken)
        {
            var result = await _db.Value.OverlappingTrainingDateRequests
                .Where(p => p.DraftApprenticeshipId == request.DraftApprenticeshipId && p.Status == OverlappingTrainingDateRequestStatus.Pending)
                .Select(oltd => new GetPendingOverlapRequestsQueryResult(oltd.DraftApprenticeshipId, oltd.PreviousApprenticeshipId, oltd.CreatedOn))
                .SingleOrDefaultAsync(cancellationToken);
                

            return result;
        }
    }
}
