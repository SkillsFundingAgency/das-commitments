using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain
{
    public class GetChangeOfProviderChainQueryHandler : IRequestHandler<GetChangeOfProviderChainQuery, GetChangeOfProviderChainQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetChangeOfProviderChainQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

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
                // to the employer requesting the provider chain
                changeOfProviderChain = changeOfProviderChain
                    .Where(r => r.EmployerAccountId == initialLink.EmployerAccountId)
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
                changeOfProviderChain.Add(link);

                await BuildChangeOfPartyRequestBackwardsChain(link.ContinuationOfId, changeOfProviderChain, cancellationToken);
            }
        }

        private async Task BuildChangeOfPartyRequestForwardsChain(long? newApprenticeshipId, List<GetChangeOfProviderChainQueryResult.ChangeOfProviderLink> changeOfProviderChain, CancellationToken cancellationToken)
        {
            if (newApprenticeshipId.HasValue)
            {
                var link = await GetChangeOfPartyRequestLink(newApprenticeshipId.Value, cancellationToken);
                changeOfProviderChain.Add(link);

                await BuildChangeOfPartyRequestForwardsChain(link.NewApprenticeshipId, changeOfProviderChain, cancellationToken);
            }
        }

        private async Task<GetChangeOfProviderChainQueryResult.ChangeOfProviderLink> GetChangeOfPartyRequestLink(long apprenticeshipId, CancellationToken cancellationToken)
        {
            var query = from a in _dbContext.Value.Apprenticeships
                        join c in _dbContext.Value.Cohorts
                            on a.CommitmentId equals c.Id
                        join p in _dbContext.Value.Providers
                            on c.ProviderId equals p.UkPrn
                        join copr in _dbContext.Value.ChangeOfPartyRequests
                            on a.Id equals copr.ApprenticeshipId into grouping 
                        from copr in grouping.DefaultIfEmpty() // into grouping with DefaultIfEmpty is a left join to ChangeOfPartyRequests
                        where a.Id == apprenticeshipId
                        select new GetChangeOfProviderChainQueryResult.ChangeOfProviderLink
                        {
                            ApprenticeshipId = a.Id,
                            EmployerAccountId = c.EmployerAccountId,
                            ProviderName = p.Name,
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            StopDate = a.StopDate,
                            ContinuationOfId = a.ContinuationOfId,
                            NewApprenticeshipId = copr.NewApprenticeshipId,
                            CreatedOn = (DateTime?)copr.CreatedOn
                        };


            var results = await query
                .FirstOrDefaultAsync(cancellationToken);

            return results;
        }
    }
}
