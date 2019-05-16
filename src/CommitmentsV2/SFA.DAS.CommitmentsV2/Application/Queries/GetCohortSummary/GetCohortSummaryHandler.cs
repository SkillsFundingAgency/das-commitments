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

        public Task<GetCohortSummaryResponse> Handle(GetCohortSummaryRequest request, CancellationToken cancellationToken)
        {
            return _dbContext.Value
                .Commitment
                .Where(c => c.Id == request.CohortId)
                .Select(c => new GetCohortSummaryResponse
                {
                    AccountLegalEntityPublicHashedId = c.AccountLegalEntityPublicHashedId,
                    CohortId = c.Id,
                    LegalEntityName = c.LegalEntityName
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
