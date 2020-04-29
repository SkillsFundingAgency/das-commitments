using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests
{
    public class GetChangeOfPartyRequestsQueryHandler : IRequestHandler<GetChangeOfPartyRequestsQuery, GetChangeOfPartyRequestsQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetChangeOfPartyRequestsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetChangeOfPartyRequestsQueryResult> Handle(GetChangeOfPartyRequestsQuery request, CancellationToken cancellationToken)
        {
            return new GetChangeOfPartyRequestsQueryResult
            {
                ChangeOfPartyRequests = await _dbContext.Value
                    .ChangeOfPartyRequests.Where(x => x.ApprenticeshipId == request.ApprenticeshipId)
                    .Select(r => new GetChangeOfPartyRequestsQueryResult.ChangeOfPartyRequest
                    {
                        Id = r.Id,
                        OriginatingParty = r.OriginatingParty,
                        ChangeOfPartyType = r.ChangeOfPartyType,
                        Status = r.Status,
                        StartDate = r.StartDate,
                        Price = r.Price,
                        EmployerName = r.AccountLegalEntity.Name,
                        CohortId = r.CohortId,
                        WithParty = r.CohortId.HasValue ? r.Cohort.WithParty : default(Party?)
                    }).ToListAsync(cancellationToken)
            };
        }
    }
}
