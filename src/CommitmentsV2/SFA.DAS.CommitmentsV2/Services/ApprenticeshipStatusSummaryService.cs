using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services;

public class ApprenticeshipStatusSummaryService(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<ApprenticeshipStatusSummaryService> logger)
    : IApprenticeshipStatusSummaryService
{
    public async Task<GetApprenticeshipStatusSummaryQueryResults> GetApprenticeshipStatusSummary(long accountId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting Apprenticeship Status Summary for employer account {AccountId}", accountId);

        var results = await dbContext.Value.AccountLegalEntities
            .Include(t => t.Cohorts)
            .ThenInclude(c => c.Apprenticeships)
            .Where(w => w.AccountId == accountId) 
            .ToListAsync(cancellationToken);

        logger.LogInformation(results.Count != 0 
            ? "Retrieved Apprenticeship Status Summary for employer account {AccountId}" 
            : "Cannot find Apprenticeship Status Summary for employer account {AccountId}", accountId);

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