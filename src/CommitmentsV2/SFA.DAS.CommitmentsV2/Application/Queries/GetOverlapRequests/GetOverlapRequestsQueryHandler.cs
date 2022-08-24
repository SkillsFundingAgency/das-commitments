using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlapRequests
{
    public class GetOverlapRequestsQueryHandler : IRequestHandler<GetOverlapRequestsQuery, GetOverlapRequestsQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetOverlapRequestsQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetOverlapRequestsQueryResult> Handle(GetOverlapRequestsQuery request, CancellationToken cancellationToken)
        {
            var result = await _db.Value.OverlappingTrainingDateRequests
                .Where(p => p.DraftApprenticeshipId == request.DraftApprenticeshipId)
                .Select(p => new GetOverlapRequestsQueryResult(p.DraftApprenticeshipId, p.PreviousApprenticeshipId, p.CreatedOn))
                .SingleOrDefaultAsync(cancellationToken);

            return result;
        }
    }
}
