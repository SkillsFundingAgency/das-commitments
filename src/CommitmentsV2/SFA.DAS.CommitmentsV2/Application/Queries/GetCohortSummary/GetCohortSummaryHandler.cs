using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.HashingService;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryHandler : IRequestHandler<GetCohortSummaryRequest, GetCohortSummaryResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetCohortSummaryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetCohortSummaryResponse> Handle(GetCohortSummaryRequest request, CancellationToken cancellationToken)
        {
            var result = await _dbContext.Value
                .Commitment
                .Where(c => c.Id == request.CohortId)
                // need an intermediate result so we can convert the varchar account legal entity
                // id (which is actually called legal entity id) to a long
                .Select(c => new 
                {
                    c.LegalEntityId,
                    c.Id,
                    c.LegalEntityName
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new GetCohortSummaryResponse
            {
                AccountLegalEntityId = long.Parse(result.LegalEntityId),
                LegalEntityName = result.LegalEntityName,
                CohortId = result.Id
            };
        }
    }
}
