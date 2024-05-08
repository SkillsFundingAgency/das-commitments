using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Expressions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;

public class GetProviderPaymentsPriorityQueryHandler : IRequestHandler<GetProviderPaymentsPriorityQuery, GetProviderPaymentsPriorityQueryResult>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly ILogger<GetProviderPaymentsPriorityQueryHandler> _logger;

    public GetProviderPaymentsPriorityQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
        ILogger<GetProviderPaymentsPriorityQueryHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetProviderPaymentsPriorityQueryResult> Handle(GetProviderPaymentsPriorityQuery message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Provider Payment Priority for employer account {EmployerAccountId}", message.EmployerAccountId);

        // get the approved providers and their current custom provider payment priority order (if any)
        // ordered by their approved on date for the given employer account
        var query = from customProviderPaymentPriority in 
            (
                from cohort in _dbContext.Value.Cohorts.Where(CohortQueries.IsFullyApproved())
                join provider in _dbContext.Value.Providers
                    on cohort.ProviderId equals provider.UkPrn
                join apprenticeship in _dbContext.Value.Apprenticeships
                    on cohort.Id equals apprenticeship.CommitmentId
                join customProviderPaymentPriority in _dbContext.Value.CustomProviderPaymentPriorities
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
            } into groupedCustomProviderPaymentPriorities
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

        _logger.LogInformation("Retrieved {PriorityItemsCount} Provider Payment Priorities for employer account {EmployerAccountId}", response.PriorityItems.Count, message.EmployerAccountId);
            
        return response;
    }
}