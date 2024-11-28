using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Expressions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;

public class GetApprovedProvidersQueryHandler(Lazy<ProviderCommitmentsDbContext> db) : IRequestHandler<GetApprovedProvidersQuery, GetApprovedProvidersQueryResult>
{
    public async Task<GetApprovedProvidersQueryResult> Handle(GetApprovedProvidersQuery request, CancellationToken cancellationToken)
    {
        var accountQuery = PredicateBuilder.True<Cohort>().And(c => c.EmployerAccountId == request.AccountId);

        var result = await db.Value.Cohorts.Where(accountQuery.And(CohortQueries.IsFullyApproved()))
            .Select(x => x.ProviderId).ToListAsync(cancellationToken);

        return new GetApprovedProvidersQueryResult(result);
    }
}