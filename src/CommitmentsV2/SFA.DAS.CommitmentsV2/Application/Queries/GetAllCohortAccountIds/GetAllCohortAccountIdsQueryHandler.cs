using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllCohortAccountIds
{
    public class GetAllCohortAccountIdsQueryHandler : IRequestHandler<GetAllCohortAccountIdsQuery, GetAllCohortAccountIdsQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<GetAllCohortAccountIdsQueryHandler> _logger;

        public GetAllCohortAccountIdsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<GetAllCohortAccountIdsQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<GetAllCohortAccountIdsQueryResult> Handle(GetAllCohortAccountIdsQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting all Cohort Account Ids");
            var accountIds = await _dbContext.Value.Cohorts
               .Select(a => a.EmployerAccountId)
               .Distinct()
               .ToListAsync( cancellationToken: cancellationToken);

            return new GetAllCohortAccountIdsQueryResult
            {
              AccountIds = accountIds
            };
        }
    }
}
