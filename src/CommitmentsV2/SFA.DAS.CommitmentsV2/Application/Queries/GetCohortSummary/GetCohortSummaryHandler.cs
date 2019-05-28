using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;

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
                .GetById(request.CohortId, c => new GetCohortSummaryResponse
                {
                    LegalEntityName = c.LegalEntityName,
                    CohortId = c.Id,
                    AccountId = c.EmployerAccountId,
                    ProviderId = c.ProviderId,
                    TransferSenderAccountId = c.TransferSenderId
                }, cancellationToken);
        }
    }
}
