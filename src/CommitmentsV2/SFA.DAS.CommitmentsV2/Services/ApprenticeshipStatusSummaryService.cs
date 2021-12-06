using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models;

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

            var result = await _dbContext.Value.Apprenticeships
                              .Include(t => t.Cohort)
                              .ThenInclude(c => c.AccountLegalEntity)
                              .Where(w => w.Cohort.EmployerAccountId == accountId) 
                              .Select(x => new ApprenticeshipSummary
                              {
                                AccountLegalEntityId = x.Cohort.AccountLegalEntity.LegalEntityId,
                                OrganisationType = x.Cohort.AccountLegalEntity.OrganisationType,
                                PaymentStatus = x.PaymentStatus

                              }).ToListAsync();

            if (result.Count() > 0)
            {
                _logger.LogInformation($"Retrieved Apprenticeship Status Summary for employer account {accountId}");
            }
            else
            {
                _logger.LogInformation($"Cannot find Apprenticeship Status Summary for employer account {accountId}");
            }

            return new GetApprenticeshipStatusSummaryQueryResults
            {
               GetApprenticeshipStatusSummaryQueryResult = new List<GetApprenticeshipStatusSummaryQueryResult>
               {
                   new GetApprenticeshipStatusSummaryQueryResult
                   {
                       LegalEntityIdentifier = result.FirstOrDefault().AccountLegalEntityId,
                       LegalEntityOrganisationType = result.FirstOrDefault().OrganisationType,
                       ActiveCount = result.Where(x => x.PaymentStatus == PaymentStatus.Active).Count(),
                       WithdrawnCount = result.Where(x => x.PaymentStatus == PaymentStatus.Withdrawn).Count(),
                       CompletedCount = result.Where(x => x.PaymentStatus == PaymentStatus.Completed).Count(),
                       PausedCount = result.Where(x => x.PaymentStatus == PaymentStatus.Paused).Count()
                   }
               }
              
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
