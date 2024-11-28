using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;

public class GetChangeOfProviderChainQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetChangeOfProviderChainQuery, GetChangeOfProviderChainQueryResult>
{
    public async Task<GetChangeOfProviderChainQueryResult> Handle(GetChangeOfProviderChainQuery request, CancellationToken cancellationToken)
    {
        var changeOfProviderChain = await BuildChangeOfProviderChain(request.ApprenticeshipId, cancellationToken);

        return new GetChangeOfProviderChainQueryResult
        {
            ChangeOfProviderChain = changeOfProviderChain
                .ToList()
        };
    }

    private async Task<List<GetChangeOfProviderChainQueryResult.ChangeOfProviderLink>> BuildChangeOfProviderChain(long apprenticeshipId, CancellationToken cancellationToken)
    {
        var changeOfProviderChain = new List<GetChangeOfProviderChainQueryResult.ChangeOfProviderLink>();

        var initialLink = await GetChangeOfPartyRequestLink(apprenticeshipId, cancellationToken);
        if (initialLink != null)
        {
            changeOfProviderChain.Add(initialLink);

            await BuildChangeOfPartyRequestBackwardsChain(initialLink.ContinuationOfId, changeOfProviderChain, cancellationToken);
            await BuildChangeOfPartyRequestForwardsChain(initialLink.NewApprenticeshipId, changeOfProviderChain, cancellationToken);

            // after the chain has been built links are removed where the employer account
            // is different to the initial link as these are not links which are visible
            // to the employer requesting the provider chain and any links were the employer
            // has been deleted as apprenticeships for these removed organisations are not visible
            changeOfProviderChain = changeOfProviderChain
                .Where(r => r.EmployerAccountId == initialLink.EmployerAccountId && !r.EmployerIsDeleted)
                .OrderByDescending(o => o.CreatedOn ?? DateTime.MaxValue)
                .ToList();
        }

        return changeOfProviderChain;
    }

    private async Task BuildChangeOfPartyRequestBackwardsChain(long? continuationOfId, List<GetChangeOfProviderChainQueryResult.ChangeOfProviderLink> changeOfProviderChain, CancellationToken cancellationToken)
    {
        if(continuationOfId.HasValue)
        { 
            var link = await GetChangeOfPartyRequestLink(continuationOfId.Value, cancellationToken);
            if (link != null)
            {
                changeOfProviderChain.Add(link);
                await BuildChangeOfPartyRequestBackwardsChain(link.ContinuationOfId, changeOfProviderChain, cancellationToken);
            }
        }
    }

    private async Task BuildChangeOfPartyRequestForwardsChain(long? newApprenticeshipId, List<GetChangeOfProviderChainQueryResult.ChangeOfProviderLink> changeOfProviderChain, CancellationToken cancellationToken)
    {
        if (newApprenticeshipId.HasValue)
        {
            var link = await GetChangeOfPartyRequestLink(newApprenticeshipId.Value, cancellationToken);
            if (link != null)
            {
                changeOfProviderChain.Add(link);
                await BuildChangeOfPartyRequestForwardsChain(link.NewApprenticeshipId, changeOfProviderChain, cancellationToken);
            }
        }
    }

    private async Task<GetChangeOfProviderChainQueryResult.ChangeOfProviderLink> GetChangeOfPartyRequestLink(long apprenticeshipId, CancellationToken cancellationToken)
    {
        var query = from a in dbContext.Value.Apprenticeships
            join c in dbContext.Value.Cohorts
                on a.CommitmentId equals c.Id
            join ale in dbContext.Value.AccountLegalEntities.IgnoreQueryFilters() // retrieve soft deleted account legal entities
                on c.AccountLegalEntityId equals ale.Id
            join p in dbContext.Value.Providers
                on c.ProviderId equals p.UkPrn
            join copr in dbContext.Value.ChangeOfPartyRequests
                on a.Id equals copr.ApprenticeshipId into grouping 
            from copr in grouping.DefaultIfEmpty() // into grouping with DefaultIfEmpty is a left join to ChangeOfPartyRequests
            where a.Id == apprenticeshipId
            select new GetChangeOfProviderChainQueryResult.ChangeOfProviderLink
            {
                ApprenticeshipId = a.Id,
                EmployerAccountId = c.EmployerAccountId,
                EmployerIsDeleted = ale.Deleted.HasValue,
                ProviderName = p.Name,
                StartDate = a.ActualStartDate ?? a.StartDate,
                EndDate = a.EndDate,
                StopDate = a.StopDate,
                ContinuationOfId = a.ContinuationOfId,
                NewApprenticeshipId = copr.NewApprenticeshipId,
                CreatedOn = copr.CreatedOn
            };

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}