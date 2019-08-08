using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryQueryHandler : IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetCohortSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public Task<GetCohortSummaryQueryResult> Handle(GetCohortSummaryQuery request, CancellationToken cancellationToken)
        {
            return (
                from c in _db.Value.Cohorts
                where c.Id == request.CohortId
                let messages = c.Messages.OrderByDescending(m => m.CreatedDateTime)
                let latestMessageCreatedByEmployer = messages.Where(m => m.CreatedBy == 0).Select(m => m.Text).FirstOrDefault()
                let latestMessageCreatedByProvider = messages.Where(m => m.CreatedBy == 1).Select(m => m.Text).FirstOrDefault()
                select new GetCohortSummaryQueryResult
                {
                    CohortId = c.Id,
                    LegalEntityName = c.LegalEntityName,
                    ProviderName = c.ProviderName,
                    IsFundedByTransfer = c.TransferSenderId != null,
                    WithParty = c.EditStatus.ToParty(),
                    LatestMessageCreatedByEmployer = latestMessageCreatedByEmployer,
                    LatestMessageCreatedByProvider = latestMessageCreatedByProvider
                })
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}