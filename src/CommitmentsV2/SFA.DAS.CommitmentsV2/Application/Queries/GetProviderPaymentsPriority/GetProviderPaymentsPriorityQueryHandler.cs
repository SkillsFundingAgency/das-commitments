using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Expressions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;

public class GetProviderPaymentsPriorityQueryHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<GetProviderPaymentsPriorityQueryHandler> logger)
    : IRequestHandler<GetProviderPaymentsPriorityQuery, GetProviderPaymentsPriorityQueryResult>
{
    public async Task<GetProviderPaymentsPriorityQueryResult> Handle(GetProviderPaymentsPriorityQuery message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting Provider Payment Priority for employer account {EmployerAccountId}", message.EmployerAccountId);

        // get the approved providers and their current custom provider payment priority order (if any)
        // ordered by their approved on date for the given employer account
        var query = from customProviderPaymentPriority in
            (
                from cohort in dbContext.Value.Cohorts.Where(CohortQueries.IsFullyApproved())
                join provider in dbContext.Value.Providers
                    on cohort.ProviderId equals provider.UkPrn
                join apprenticeship in dbContext.Value.Apprenticeships
                    on cohort.Id equals apprenticeship.CommitmentId
                join customProviderPaymentPriority in dbContext.Value.CustomProviderPaymentPriorities
                    on new
                    {
                        cohort.ProviderId,
                        cohort.EmployerAccountId
                    }
                    equals new
                    {
                        customProviderPaymentPriority.ProviderId,
                        customProviderPaymentPriority.EmployerAccountId
                    } into customProviderPaymentPriorities
                from customProviderPaymentPriority in customProviderPaymentPriorities.DefaultIfEmpty()
                where cohort.EmployerAccountId == message.EmployerAccountId
                select new
                {
                    PriorityDate = apprenticeship.AgreedOn ?? cohort.EmployerAndProviderApprovedOn,
                    ProviderName = provider.Name,
                    cohort.EmployerAccountId,
                    cohort.ProviderId,
                    PriorityOrder = (int?)customProviderPaymentPriority.PriorityOrder
                }
            )
            group customProviderPaymentPriority by new
            {
                customProviderPaymentPriority.ProviderName,
                customProviderPaymentPriority.ProviderId,
                customProviderPaymentPriority.EmployerAccountId,
                customProviderPaymentPriority.PriorityOrder
            }
            into groupedCustomProviderPaymentPriorities
            orderby groupedCustomProviderPaymentPriorities.Min(x => x.PriorityDate)
            select new
            {
                groupedCustomProviderPaymentPriorities.Key.ProviderName,
                groupedCustomProviderPaymentPriorities.Key.ProviderId,
                groupedCustomProviderPaymentPriorities.Key.EmployerAccountId,
                groupedCustomProviderPaymentPriorities.Key.PriorityOrder
            };

        var results = await query.ToListAsync(cancellationToken: cancellationToken);

        // for those providers which have a custom provider payment priority, the priority order will
        // be preserved regardless of their approved on date, with all those providers without a custom
        // provider payment priority included afterwards ordered by their approved on date
        var response = new GetProviderPaymentsPriorityQueryResult
        {
            PriorityItems = results.Select((p, index) => new
                {
                    p.ProviderName,
                    p.ProviderId,
                    PriorityOrder = p.PriorityOrder ?? index + 101
                })
                .OrderBy(p => p.PriorityOrder)
                .Select((p, index) => new GetProviderPaymentsPriorityQueryResult.ProviderPaymentsPriorityItem
                {
                    ProviderName = p.ProviderName,
                    ProviderId = p.ProviderId,
                    PriorityOrder = index + 1
                })
                .ToList()
        };

        logger.LogInformation("Retrieved {PriorityItemsCount} Provider Payment Priorities for employer account {EmployerAccountId}", response.PriorityItems.Count, message.EmployerAccountId);

        return response;
    }
}