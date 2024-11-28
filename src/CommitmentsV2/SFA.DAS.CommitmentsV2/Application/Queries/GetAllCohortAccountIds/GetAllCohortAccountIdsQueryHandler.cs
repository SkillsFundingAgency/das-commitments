using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllCohortAccountIds;

public class GetAllCohortAccountIdsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<GetAllCohortAccountIdsQueryHandler> logger)
    : IRequestHandler<GetAllCohortAccountIdsQuery, GetAllCohortAccountIdsQueryResult>
{
    public async Task<GetAllCohortAccountIdsQueryResult> Handle(GetAllCohortAccountIdsQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all Cohort Account Ids");
        
        var accountIds = await dbContext.Value.Cohorts
            .Select(a => a.EmployerAccountId)
            .Distinct()
            .ToListAsync( cancellationToken: cancellationToken);

        return new GetAllCohortAccountIdsQueryResult
        {
            AccountIds = accountIds
        };
    }
}