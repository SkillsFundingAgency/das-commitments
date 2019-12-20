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
    public class GetAccountSummaryHandler : IRequestHandler<GetAccountSummaryRequest, GetAccountSummaryResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAccountSummaryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetAccountSummaryResponse> Handle(GetAccountSummaryRequest request,
            CancellationToken cancellationToken)
        {
            var accountQuery = PredicateBuilder.True<Cohort>().And(c => c.EmployerAccountId == request.AccountId);

            var hasCohorts = await _dbContext.Value.Cohorts
                .AnyAsync(accountQuery.And(CohortQueries.IsNotFullyApproved()), cancellationToken: cancellationToken);

            var hasApprenticeships = await _dbContext.Value.Apprenticeships
                .AnyAsync(a => a.Cohort.EmployerAccountId == request.AccountId, cancellationToken: cancellationToken);

            return new GetAccountSummaryResponse
            {
                AccountId = request.AccountId,
                HasCohorts = hasCohorts,
                HasApprenticeships = hasApprenticeships
            };
        }
    }
}
