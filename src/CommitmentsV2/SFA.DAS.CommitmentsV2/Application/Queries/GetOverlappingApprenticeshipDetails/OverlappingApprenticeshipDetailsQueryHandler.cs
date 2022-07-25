using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails
{
    public class GetOverlappingApprenticeshipDetailsQueryHandler : IRequestHandler<GetOverlappingApprenticeshipDetailsQuery, GetOverlappingApprenticeshipDetailsQueryResult>
    {
        private Lazy<ProviderCommitmentsDbContext> _providerDbContext;
        private IOverlapCheckService _overlapCheckService;
        private ILogger<GetOverlappingApprenticeshipDetailsQueryHandler> _logger;

        public GetOverlappingApprenticeshipDetailsQueryHandler(IOverlapCheckService overlapCheckService,
            Lazy<ProviderCommitmentsDbContext> providerDbContext,
            ILogger<GetOverlappingApprenticeshipDetailsQueryHandler> logger)
        {
            _providerDbContext = providerDbContext;
            _overlapCheckService = overlapCheckService;
            _logger = logger;
        }

        public async Task<GetOverlappingApprenticeshipDetailsQueryResult> Handle(GetOverlappingApprenticeshipDetailsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting overlapping apprenticeship information draft apprenticeship {request.DraftApprenticeshipId}");
            var draftApprenticeShip = await _providerDbContext.Value.DraftApprenticeships.SingleOrDefaultAsync(x => x.Id == request.DraftApprenticeshipId);

            if (draftApprenticeShip == null 
                || !draftApprenticeShip.StartDate.HasValue 
                || !draftApprenticeShip.EndDate.HasValue 
                || string.IsNullOrWhiteSpace(draftApprenticeShip.Uln))
            {
                throw new InvalidOperationException($"No draft apprenticeship found for this draft apprenticeship :{request.DraftApprenticeshipId} ");
            }

            var apprenticeshipWithOverlap = await _overlapCheckService.CheckForOverlapsOnStartDate(draftApprenticeShip.Uln, new Domain.Entities.DateRange(draftApprenticeShip.StartDate.Value, draftApprenticeShip.EndDate.Value), null, cancellationToken);
            if (!apprenticeshipWithOverlap.ApprenticeshipId.HasValue ||  !apprenticeshipWithOverlap.HasOverlappingStartDate)
            {
                throw new InvalidOperationException($"No start date overlap found for this draft apprenticeship: {draftApprenticeShip.Id}");
            }

            var apprenticeship = await _providerDbContext.Value.Apprenticeships.SingleOrDefaultAsync(x => x.Id == apprenticeshipWithOverlap.ApprenticeshipId);

            return await Task.FromResult(new GetOverlappingApprenticeshipDetailsQueryResult { ApprenticeshipId = apprenticeship.Id, Status = apprenticeship.ApprenticeshipStatus });
        }
    }
}
