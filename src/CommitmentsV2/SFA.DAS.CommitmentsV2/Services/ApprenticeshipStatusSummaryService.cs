using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ApprenticeshipStatusSummaryService : IApprenticeshipStatusSummaryService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<ApprenticeshipStatusSummaryService> _logger;

        public ApprenticeshipStatusSummaryService(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<ApprenticeshipStatusSummaryService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<GetApprenticeshipStatusSummaryQueryResults> GetApprenticeshipStatusSummary(long accountId, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting Apprenticeship Status Summary for employer account {accountId}");

            var results = await _dbContext.Value.AccountLegalEntities
                              .Include(t => t.Cohorts)
                              .ThenInclude(c => c.Apprenticeships)
                              .Where(w => w.AccountId == accountId) 
                              .ToListAsync();

            if (results.Any())
            {
                _logger.LogInformation($"Retrieved Apprenticeship Status Summary for employer account {accountId}");
            }
            else
            {
                _logger.LogInformation($"Cannot find Apprenticeship Status Summary for employer account {accountId}");                
            }           

            return new GetApprenticeshipStatusSummaryQueryResults
            {
                GetApprenticeshipStatusSummaryQueryResult = results.Select(x => new GetApprenticeshipStatusSummaryQueryResult
                {
                    LegalEntityIdentifier = x.LegalEntityId,
                    LegalEntityOrganisationType = x.OrganisationType,
                    ActiveCount = x.Cohorts.SelectMany(c => c.Apprenticeships).Count(apprenticeship => apprenticeship.PaymentStatus == PaymentStatus.Active),
                    WithdrawnCount = x.Cohorts.SelectMany(c => c.Apprenticeships).Count(apprenticeship => apprenticeship.PaymentStatus == PaymentStatus.Withdrawn),
                    CompletedCount = x.Cohorts.SelectMany(c => c.Apprenticeships).Count(apprenticeship => apprenticeship.PaymentStatus == PaymentStatus.Completed),
                    PausedCount = x.Cohorts.SelectMany(c => c.Apprenticeships).Count(apprenticeship => apprenticeship.PaymentStatus == PaymentStatus.Paused)
                })
               
            };  
        }
    }
}
