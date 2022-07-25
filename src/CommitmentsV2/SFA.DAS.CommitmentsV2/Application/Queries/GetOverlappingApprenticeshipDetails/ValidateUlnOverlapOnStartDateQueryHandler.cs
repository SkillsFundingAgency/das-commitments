using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails
{
    public class ValidateUlnOverlapOnStartDateQueryHandler : IRequestHandler<ValidateUlnOverlapOnStartDateQuery, ValidateUlnOverlapOnStartDateQueryResult>
    {
       // private Lazy<ProviderCommitmentsDbContext> _providerDbContext;
        private IOverlapCheckService _overlapCheckService;
        private ILogger<ValidateUlnOverlapOnStartDateQueryHandler> _logger;

        public ValidateUlnOverlapOnStartDateQueryHandler(IOverlapCheckService overlapCheckService,
           // Lazy<ProviderCommitmentsDbContext> providerDbContext,
            ILogger<ValidateUlnOverlapOnStartDateQueryHandler> logger)
        {
         //   _providerDbContext = providerDbContext;
            _overlapCheckService = overlapCheckService;
            _logger = logger;
        }

        public async Task<ValidateUlnOverlapOnStartDateQueryResult> Handle(ValidateUlnOverlapOnStartDateQuery request, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"Getting overlapping apprenticeship information draft apprenticeship {request.DraftApprenticeshipId}");
            //var draftApprenticeShip = await _providerDbContext.Value.DraftApprenticeships.SingleOrDefaultAsync(x => x.Id == request.DraftApprenticeshipId);

            //if (draftApprenticeShip == null 
            //    || !draftApprenticeShip.StartDate.HasValue 
            //    || !draftApprenticeShip.EndDate.HasValue 
            //    || string.IsNullOrWhiteSpace(draftApprenticeShip.Uln))
            //{
            //    throw new InvalidOperationException($"No draft apprenticeship found for this draft apprenticeship :{request.DraftApprenticeshipId} ");
            //}
            var stDate = System.DateTime.ParseExact(request.StartDate, "dd-MM-yyyy", null);
            var edDate = System.DateTime.ParseExact(request.EndDate, "dd-MM-yyyy", null);

            var apprenticeshipWithOverlap = await _overlapCheckService.CheckForOverlapsOnStartDate(request.Uln, new Domain.Entities.DateRange(stDate, edDate), null, cancellationToken);

            var result = new ValidateUlnOverlapOnStartDateQueryResult { HasStartDateOverlap = apprenticeshipWithOverlap.HasOverlappingStartDate, HasOverlapWithApprenticeshipId = apprenticeshipWithOverlap.ApprenticeshipId };

            return await Task.FromResult(result);
        }
    }
}
