using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfEmployerChain
{
    public class GetChangeOfEmployerChainQueryHandler : IRequestHandler<GetChangeOfEmployerChainQuery, GetChangeOfEmployerChainQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetChangeOfEmployerChainQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetChangeOfEmployerChainQueryResult> Handle(GetChangeOfEmployerChainQuery request, CancellationToken cancellationToken)
        {
            var changeOfEmployerChain = await BuildChangeOfEmployerChain(request.ApprenticeshipId, cancellationToken);

            return new GetChangeOfEmployerChainQueryResult
            {
                ChangeOfEmployerChain = changeOfEmployerChain.ToList()
            };
        }

        private async Task<List<GetChangeOfEmployerChainQueryResult.ChangeOfEmployerLink>> BuildChangeOfEmployerChain(long apprenticeshipId, CancellationToken cancellationToken)
        {
            var changeOfEmployerChain = new List<GetChangeOfEmployerChainQueryResult.ChangeOfEmployerLink>();

            var initialLink = await GetChangeOfPartyRequestLink(apprenticeshipId, cancellationToken);
            if (initialLink != null)
            {
                changeOfEmployerChain.Add(initialLink);

                await BuildChangeOfPartyRequestBackwardsChain(initialLink.ContinuationOfId, changeOfEmployerChain, cancellationToken);
                await BuildChangeOfPartyRequestForwardsChain(initialLink.NewApprenticeshipId, changeOfEmployerChain, cancellationToken);

                // after the chain has been built links are removed where the provider account
                // is different to the initial link as these are not links which are visible
                // to the provider requesting the employer chain and any links were the employer
                // has been deleted as apprenticeships for these removed organisations are not visible
                changeOfEmployerChain = changeOfEmployerChain
                    .Where(r => r.Ukprn == initialLink.Ukprn && !r.EmployerIsDeleted)
                    .OrderByDescending(o => o.CreatedOn ?? DateTime.MaxValue)
                    .ToList();
            }

            return changeOfEmployerChain;
        }

        private async Task BuildChangeOfPartyRequestBackwardsChain(long? continuationOfId, List<GetChangeOfEmployerChainQueryResult.ChangeOfEmployerLink> changeOfEmployerChain, CancellationToken cancellationToken)
        {
            if(continuationOfId.HasValue)
            { 
                var link = await GetChangeOfPartyRequestLink(continuationOfId.Value, cancellationToken);
                if (link != null)
                {
                    changeOfEmployerChain.Add(link);
                    await BuildChangeOfPartyRequestBackwardsChain(link.ContinuationOfId, changeOfEmployerChain, cancellationToken);
                }
            }
        }

        private async Task BuildChangeOfPartyRequestForwardsChain(long? newApprenticeshipId, List<GetChangeOfEmployerChainQueryResult.ChangeOfEmployerLink> changeOfEmployerChain, CancellationToken cancellationToken)
        {
            if (newApprenticeshipId.HasValue)
            {
                var link = await GetChangeOfPartyRequestLink(newApprenticeshipId.Value, cancellationToken);
                if (link != null)
                {
                    changeOfEmployerChain.Add(link);
                    await BuildChangeOfPartyRequestForwardsChain(link.NewApprenticeshipId, changeOfEmployerChain, cancellationToken);
                }
            }
        }

        private async Task<GetChangeOfEmployerChainQueryResult.ChangeOfEmployerLink> GetChangeOfPartyRequestLink(long apprenticeshipId, CancellationToken cancellationToken)
        {
            var query = from a in _dbContext.Value.Apprenticeships
                        join c in _dbContext.Value.Cohorts
                            on a.CommitmentId equals c.Id
                        join ale in _dbContext.Value.AccountLegalEntities.IgnoreQueryFilters() // retrieve soft deleted account legal entities
                            on c.AccountLegalEntityId equals ale.Id
                        join copr in _dbContext.Value.ChangeOfPartyRequests
                            on a.Id equals copr.ApprenticeshipId into grouping 
                        from copr in grouping.DefaultIfEmpty() // into grouping with DefaultIfEmpty is a left join to ChangeOfPartyRequests
                        where a.Id == apprenticeshipId
                        select new GetChangeOfEmployerChainQueryResult.ChangeOfEmployerLink
                        {
                            ApprenticeshipId = a.Id,
                            Ukprn = c.ProviderId,
                            EmployerIsDeleted = ale.Deleted.HasValue,
                            EmployerName = ale.Name,
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            StopDate = a.StopDate,
                            ContinuationOfId = a.ContinuationOfId,
                            NewApprenticeshipId = copr.NewApprenticeshipId,
                            CreatedOn = (DateTime?)copr.CreatedOn
                        };

            var result = await query
                .FirstOrDefaultAsync(cancellationToken);

            return result;
        }
    }
}
