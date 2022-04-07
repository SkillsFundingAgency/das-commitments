using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                    ActiveCount = x.Cohorts.SelectMany(c => c.Apprenticeships).Where(x => x.PaymentStatus == PaymentStatus.Active).Count(),
                    WithdrawnCount = x.Cohorts.SelectMany(c => c.Apprenticeships).Where(x => x.PaymentStatus == PaymentStatus.Withdrawn).Count(),
                    CompletedCount = x.Cohorts.SelectMany(c => c.Apprenticeships).Where(x => x.PaymentStatus == PaymentStatus.Completed).Count(),
                    PausedCount = x.Cohorts.SelectMany(c => c.Apprenticeships).Where(x => x.PaymentStatus == PaymentStatus.Paused).Count()
                })
               
            };  
        }

        public async Task<GetApprenticeshipStatisticsQueryResult> GetApprenticeshipStatisticsFor(int lastNumberOfDays)
        {
            var fromDate = DateTime.UtcNow.AddDays(Math.Abs(lastNumberOfDays) * -1).Date;

            var commitmentsApprovedCount = await _dbContext.Value
                .Apprenticeships
                .Include(x => x.Cohort)
                .CountAsync(x =>
                    x.Cohort.EmployerAndProviderApprovedOn > fromDate &&
                    (x.Cohort.Approvals == (Party)3 || x.Cohort.Approvals == (Party)7));

            var commitmentsStoppedCount = await _dbContext.Value
                .Apprenticeships
                .CountAsync(x =>
                    x.StopDate > fromDate &&
                    x.PaymentStatus == PaymentStatus.Withdrawn);

            var commitmentsPausedCount = await _dbContext.Value
                .Apprenticeships
                .CountAsync(x =>
                    x.IsApproved &&
                    x.PauseDate > fromDate &&
                    x.PaymentStatus == PaymentStatus.Paused);

            return new GetApprenticeshipStatisticsQueryResult
            {
                ApprovedApprenticeshipCount = commitmentsApprovedCount,
                StoppedApprenticeshipCount = commitmentsStoppedCount,
                PausedApprenticeshipCount = commitmentsPausedCount
            };
        }
    }

    public class ApprenticeshipSummary
    {
        public string AccountLegalEntityId { get; set; }
        
        public OrganisationType OrganisationType { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public int Count { get; set; }
    }
}