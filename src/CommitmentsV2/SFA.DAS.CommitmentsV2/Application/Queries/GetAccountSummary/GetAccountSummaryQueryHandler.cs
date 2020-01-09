using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Expressions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary
{
    public class GetAccountSummaryQueryHandler : IRequestHandler<GetAccountSummaryQuery, GetAccountSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAccountSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetAccountSummaryQueryResult> Handle(GetAccountSummaryQuery query,
            CancellationToken cancellationToken)
        {
            var accountQuery = PredicateBuilder.True<Cohort>().And(c => c.EmployerAccountId == query.AccountId);

            var hasCohorts = await _dbContext.Value.Cohorts
                .AnyAsync(accountQuery.And(CohortQueries.IsNotFullyApproved()), cancellationToken: cancellationToken);

            var hasApprenticeships = await _dbContext.Value.ApprovedApprenticeships
                .AnyAsync(a => a.Cohort.EmployerAccountId == query.AccountId, cancellationToken: cancellationToken);

            return new GetAccountSummaryQueryResult
            {
                AccountId = query.AccountId,
                HasCohorts = hasCohorts,
                HasApprenticeships = hasApprenticeships
            };
        }
    }
}
