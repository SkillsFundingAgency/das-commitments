using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails
{
    /// TODO: Unit test
    public class ValidateUlnOverlapOnStartDateQueryHandler : IRequestHandler<ValidateUlnOverlapOnStartDateQuery, ValidateUlnOverlapOnStartDateQueryResult>
    {
        private IOverlapCheckService _overlapCheckService;
        private ILogger<ValidateUlnOverlapOnStartDateQueryHandler> _logger;

        public ValidateUlnOverlapOnStartDateQueryHandler(IOverlapCheckService overlapCheckService,
            ILogger<ValidateUlnOverlapOnStartDateQueryHandler> logger)
        {
            _overlapCheckService = overlapCheckService;
            _logger = logger;
        }

        public async Task<ValidateUlnOverlapOnStartDateQueryResult> Handle(ValidateUlnOverlapOnStartDateQuery request, CancellationToken cancellationToken)
        {
            var stDate = System.DateTime.ParseExact(request.StartDate, "dd-MM-yyyy", null);
            var edDate = System.DateTime.ParseExact(request.EndDate, "dd-MM-yyyy", null);

            var apprenticeshipWithOverlap = await _overlapCheckService.CheckForOverlapsOnStartDate(request.Uln, new Domain.Entities.DateRange(stDate, edDate), null, cancellationToken);

            var result = new ValidateUlnOverlapOnStartDateQueryResult { HasStartDateOverlap = apprenticeshipWithOverlap.HasOverlappingStartDate, HasOverlapWithApprenticeshipId = apprenticeshipWithOverlap.ApprenticeshipId };

            return await Task.FromResult(result);
        }
    }
}
