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
            Models.OverlappingTrainingDateRequest overlappingTrainingDateRequest = null;

            if (request.ApprenticeshipId.HasValue)
            {
              overlappingTrainingDateRequest =   await _dbContext.Value
                .OverlappingTrainingDateRequests.
                FirstOrDefaultAsync(x => x.PreviousApprenticeshipId == request.ApprenticeshipId, cancellationToken);
            }
            else if (request.DraftApprenticeshipId.HasValue)
            {
                overlappingTrainingDateRequest = await _dbContext.Value
               .OverlappingTrainingDateRequests.
               FirstOrDefaultAsync(x => x.PreviousApprenticeshipId == request.ApprenticeshipId, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Passed apprenticeship Id and draftapprenticeship id are null");
            }

            if (overlappingTrainingDateRequest == null)
                return null;

            return new GetOverlappingTrainingDateRequestQueryResult
            {
                Id = overlappingTrainingDateRequest.Id,
                DraftApprenticeshipId = overlappingTrainingDateRequest.DraftApprenticeshipId,
                PreviousApprenticeshipId = overlappingTrainingDateRequest.PreviousApprenticeshipId,
                ResolutionType = overlappingTrainingDateRequest.ResolutionType,
                Status = overlappingTrainingDateRequest.Status,
                ActionedOn = overlappingTrainingDateRequest.ActionedOn,
                CreatedOn = overlappingTrainingDateRequest.CreatedOn
            };
        }
    }
}