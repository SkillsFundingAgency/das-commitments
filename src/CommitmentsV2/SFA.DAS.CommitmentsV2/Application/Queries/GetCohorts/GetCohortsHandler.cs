using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts
{
    public class GetCohortsHandler : IRequestHandler<GetCohortsQuery, GetCohortsResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetCohortsHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetCohortsResult> Handle(GetCohortsQuery command, CancellationToken cancellationToken)
        {
            var cohorts = await _db.Value.Cohorts.Include(cohort=>cohort.Apprenticeships).Where(x => x.EmployerAccountId == command.AccountId)
                .Select(x => new CohortSummary
                {
                    AccountId = x.EmployerAccountId,
                    LegalEntityName = x.LegalEntityName,
                    ProviderId = x.ProviderId.Value,
                    ProviderName = x.ProviderName,
                    CohortId = x.Id,
                    //NumberOfDraftApprentices = x.DraftApprenticeships.Count()
                }).ToListAsync(cancellationToken);

            return new GetCohortsResult(cohorts);
        }
    }
}