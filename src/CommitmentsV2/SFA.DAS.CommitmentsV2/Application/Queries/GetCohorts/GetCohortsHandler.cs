using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts
{
    public class GetCohortsHandler : IRequestHandler<GetCohortsQuery, GetCohortsResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly ILogger<GetCohortsHandler> _logger;

        public GetCohortsHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<GetCohortsHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<GetCohortsResult> Handle(GetCohortsQuery command, CancellationToken cancellationToken)
        {
            try
            {
                var cohortFiltered = await _db.Value.Cohorts.Where(c => c.EmployerAccountId == command.AccountId &&
                       (c.EditStatus != EditStatus.Both ||
                        (c.TransferSenderId != null && c.TransferApprovalStatus != TransferApprovalStatus.Approved)))
                       .GroupJoin(_db.Value.Accounts, cohort => cohort.TransferSenderId, account => account.Id,
                  (cohort, account) => new { Cohort = cohort, TransferSender = account })
                 .Select(c => new
                 {
                     c.Cohort,
                     c.TransferSender
                 }).ToArrayAsync(cancellationToken);

                var cohortIds = cohortFiltered.Select(x => x.Cohort.Id);

                var apprenticesTask = _db.Value.DraftApprenticeships.Where(y => cohortIds.Contains(y.CommitmentId))
                    .GroupBy(x => x.CommitmentId).Select(z => new
                    {
                        CohortId = z.Key,
                        NumberOfApprentices = z.Count()
                    }).ToArrayAsync(cancellationToken);

                var filteredMessages = _db.Value.Messages.Where(y => cohortIds.Contains(y.CommitmentId))
                    .GroupBy(x => new { x.CreatedBy, x.CommitmentId })
                    .Select(z => z.Max(x => x.Id));
                var messagesTask = _db.Value.Messages.Where(m => filteredMessages.Contains(m.Id)).Select(y => new
                {
                    y.CommitmentId,
                    y.CreatedBy,
                    y.CreatedDateTime,
                    y.Text,
                }).ToArrayAsync(cancellationToken);

                var apprentices = await apprenticesTask;
                var messages = await messagesTask;

                var cohorts = cohortFiltered.GroupJoin(apprentices, c => c.Cohort.Id, a => a.CohortId,
                        (c, a) => new { c.Cohort, Apprentices = a?.FirstOrDefault(), TransferSender = c.TransferSender?.FirstOrDefault()})
                       .GroupJoin(messages, c => c.Cohort.Id, m => m.CommitmentId,
                       (c, m) => new CohortSummary
                       {
                           AccountId = c.Cohort.EmployerAccountId,
                           LegalEntityName = c.Cohort.LegalEntityName,
                           ProviderId = c.Cohort.ProviderId.Value,
                           ProviderName = c.Cohort.ProviderName,
                           CohortId = c.Cohort.Id,
                           NumberOfDraftApprentices = c.Apprentices?.NumberOfApprentices ?? 0,
                           LatestMessageFromEmployer = m?.Where(x => x.CreatedBy == 0).Select(m => new Message(m.Text, m.CreatedDateTime)).FirstOrDefault(),
                           LatestMessageFromProvider = m?.Where(x => x.CreatedBy == 1).Select(m => new Message(m.Text, m.CreatedDateTime)).FirstOrDefault(),
                           IsDraft = c.Cohort.LastAction == LastAction.None,
                           WithParty = c.Cohort.WithParty,
                           CreatedOn = c.Cohort.CreatedOn.Value,
                           TransferSenderName = c.TransferSender?.Name,
                           TransferSenderId = c.Cohort.TransferSenderId
                       }).ToList();

                return new GetCohortsResult(cohorts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
    }
}