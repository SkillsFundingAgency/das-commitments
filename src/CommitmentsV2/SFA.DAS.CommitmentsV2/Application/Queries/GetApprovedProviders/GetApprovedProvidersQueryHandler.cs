using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Expressions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders
{
    public class GetApprovedProvidersQueryHandler : IRequestHandler<GetApprovedProvidersQuery, GetApprovedProvidersQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetApprovedProvidersQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetApprovedProvidersQueryResult> Handle(GetApprovedProvidersQuery request, CancellationToken cancellationToken)
        {
            var accountQuery = PredicateBuilder.True<Cohort>().And(c => c.EmployerAccountId == request.AccountId);

            var result = await _db.Value.Cohorts.Where(accountQuery.And(CohortQueries.IsFullyApproved()))
                .Select(x => x.ProviderId).ToListAsync(cancellationToken);

            return new GetApprovedProvidersQueryResult(result);
        }
    }
}
