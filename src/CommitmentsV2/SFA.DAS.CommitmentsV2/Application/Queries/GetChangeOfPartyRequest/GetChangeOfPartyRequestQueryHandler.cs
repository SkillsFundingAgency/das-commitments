using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequest
{
    public class GetChangeOfPartyRequestQueryHandler : IRequestHandler<GetChangeOfPartyRequestQuery, GetChangeOfPartyRequestQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetChangeOfPartyRequestQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetChangeOfPartyRequestQueryResult> Handle(GetChangeOfPartyRequestQuery request, CancellationToken cancellationToken)
        {
            var changeOfPartyRequest = _dbContext.Value.ChangeOfPartyRequests.FirstOrDefault(r => r.Id == request.ChangeOfPartyRequestId);

            return Task.FromResult(new GetChangeOfPartyRequestQueryResult
            {
               Id = changeOfPartyRequest.Id,
               ApprenticeshipId = changeOfPartyRequest.ApprenticeshipId
            });
        }
    }
}
