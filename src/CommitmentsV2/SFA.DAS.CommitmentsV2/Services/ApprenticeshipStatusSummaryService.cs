using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
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

        public async Task<GetApprenticeshipStatusSummaryQueryResults> GetApprenticeshipStatusSummary(long employerAccountId, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting Apprenticeship Status Summary for employer account {employerAccountId}");

            var employerAccountIdParam = new SqlParameter("@employerAccountId", employerAccountId);
         
            var results = await _dbContext.Value.ApprenticeshipStatusSummary
                             .FromSql("EXEC GetApprenticeshipStatusSummaries @employerAccountId", employerAccountIdParam)
                             .ToListAsync();


            var apprenticeshipsStatusSummaries = new Dictionary<string, GetApprenticeshipStatusSummaryQueryResult>();

            foreach (var result in results)
            {
                var legalEntityId = result.LegalEntityId;
                var organisationType = result.LegalEntityOrganisationType;
                var paymentStatus = result.PaymentStatus;
                var count = result.Count;

                if (!apprenticeshipsStatusSummaries.ContainsKey(legalEntityId))
                {
                    apprenticeshipsStatusSummaries.Add(legalEntityId, new GetApprenticeshipStatusSummaryQueryResult
                    {
                        LegalEntityIdentifier = legalEntityId,
                        LegalEntityOrganisationType = (Api.Types.Responses.OrganisationType)organisationType
                    });
                }

                var apprenticeshipStatusSummary = apprenticeshipsStatusSummaries[legalEntityId];

                switch (result.PaymentStatus)
                {
                    case PaymentStatus.PendingApproval:
                        apprenticeshipStatusSummary.PendingApprovalCount = count;
                        break;
                    case PaymentStatus.Active:
                        apprenticeshipStatusSummary.ActiveCount = count;
                        break;
                    case PaymentStatus.Paused:
                        apprenticeshipStatusSummary.PausedCount = count;
                        break;
                    case PaymentStatus.Withdrawn:
                        apprenticeshipStatusSummary.WithdrawnCount = count;
                        break;
                    case PaymentStatus.Completed:
                        apprenticeshipStatusSummary.CompletedCount = count;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unexpected payment status '{paymentStatus}' found when creating apprenticeship summary statuses");
                }
            }

            if (apprenticeshipsStatusSummaries != null)
            {
                _logger.LogInformation($"Retrieved Apprenticeship Status Summary for employer account {employerAccountId}");
            }
            else
            {
                _logger.LogInformation($"Cannot find Apprenticeship Status Summary for employer account {employerAccountId}");
            }

            return new GetApprenticeshipStatusSummaryQueryResults
            {
                GetApprenticeshipStatusSummaryQueryResult = apprenticeshipsStatusSummaries.Values
            };
        }
    }
}
