using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest
{
    public class GetOverlappingTrainingDateRequestQueryHandler : IRequestHandler<GetOverlappingTrainingDateRequestQuery, GetOverlappingTrainingDateRequestQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetOverlappingTrainingDateRequestQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetOverlappingTrainingDateRequestQueryResult> Handle(GetOverlappingTrainingDateRequestQuery request, CancellationToken cancellationToken)
        {
            var overlappingTrainingDateRequest = await _dbContext.Value.OverlappingTrainingDateRequests
                .Where(x => x.PreviousApprenticeshipId == request.ApprenticeshipId)
                .ToListAsync(cancellationToken);

            if (overlappingTrainingDateRequest == null)
                return new GetOverlappingTrainingDateRequestQueryResult();

            return new GetOverlappingTrainingDateRequestQueryResult
            {
                OverlappingTrainingDateRequests = overlappingTrainingDateRequest.Select(x => new GetOverlappingTrainingDateRequestQueryResult.OverlappingTrainingDateRequest
                {
                    Id = x.Id,
                    DraftApprenticeshipId = x.DraftApprenticeshipId,
                    PreviousApprenticeshipId = x.PreviousApprenticeshipId,
                    ResolutionType = x.ResolutionType,
                    Status = x.Status,
                    ActionedOn = x.ActionedOn,
                    CreatedOn = x.CreatedOn,
                }).ToList()
            };
        }
    }
}