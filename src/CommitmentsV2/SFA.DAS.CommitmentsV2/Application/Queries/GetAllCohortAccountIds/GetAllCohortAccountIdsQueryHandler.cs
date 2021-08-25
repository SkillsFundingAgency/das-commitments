using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllCohortAccountIds
{
    public class GetAllCohortAccountIdsQueryHandler : IRequestHandler<GetAllCohortAccountIdsQuery, GetAllCohortAccountIdsQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAllCohortAccountIdsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetAllCohortAccountIdsQueryResult> Handle(GetAllCohortAccountIdsQuery query, CancellationToken cancellationToken)
        {
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
