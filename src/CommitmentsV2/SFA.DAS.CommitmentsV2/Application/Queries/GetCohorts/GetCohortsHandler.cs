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
            var query =
                from c in _db.Value.Cohorts
                where c.EmployerAccountId == command.AccountId &&
                      (c.EditStatus != EditStatus.Both ||
                       (c.TransferSenderId != null && c.TransferApprovalStatus != TransferApprovalStatus.Approved))
                let messages = c.Messages.OrderByDescending(m => m.CreatedDateTime)
                let latestMessageCreatedByEmployer = messages.Where(m => m.CreatedBy == 0).Select(m => m.Text).FirstOrDefault()
                let latestMessageCreatedByProvider = messages.Where(m => m.CreatedBy == 1).Select(m => m.Text).FirstOrDefault()

                select new CohortSummary
                {
                    AccountId = c.EmployerAccountId,
                    LegalEntityName = c.LegalEntityName,
                    ProviderId = c.ProviderId.Value,
                    ProviderName = c.ProviderName,
                    CohortId = c.Id,
                    NumberOfDraftApprentices = c.Apprenticeships.Count,
                    LastMessageFromEmployer = latestMessageCreatedByEmployer,
                    LastMessageFromProvider = latestMessageCreatedByProvider,
                    IsDraft = c.LastAction == LastAction.None,
                    WithParty = c.WithParty
                };

            var cohorts = await query.ToArrayAsync(cancellationToken);

            return new GetCohortsResult(cohorts);
        }
    }
}