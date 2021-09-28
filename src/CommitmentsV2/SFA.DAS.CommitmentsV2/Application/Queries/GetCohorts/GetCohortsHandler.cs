using System;
using System.Linq;
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
                //filter cohorts
                var cohortFiltered = await (from c in _db.Value.Cohorts
                                            join a in _db.Value.Accounts on c.TransferSenderId equals a.Id into account
                                            from transferSender in account.DefaultIfEmpty()
                                            where c.EmployerAccountId == (command.AccountId ?? c.EmployerAccountId) && c.ProviderId == (command.ProviderId ?? c.ProviderId) &&
                                                (c.EditStatus != EditStatus.Both ||
                                                (c.TransferSenderId != null && c.TransferApprovalStatus != TransferApprovalStatus.Approved))
                                            select new CohortSummary
                                            {
                                                AccountId = c.EmployerAccountId,
                                                LegalEntityName = c.AccountLegalEntity.Name,
                                                AccountLegalEntityPublicHashedId = c.AccountLegalEntity.PublicHashedId,
                                                ProviderId = c.ProviderId,
                                                ProviderName = c.Provider.Name,
                                                CohortId = c.Id,
                                                IsDraft = c.IsDraft,
                                                WithParty = c.WithParty,
                                                CreatedOn = c.CreatedOn.Value,
                                                TransferSenderName = transferSender.Name,
                                                TransferSenderId = c.TransferSenderId,
                                                IsLinkedToChangeOfPartyRequest = c.IsLinkedToChangeOfPartyRequest
                                            }).ToArrayAsync(cancellationToken);

                var cohortIds = cohortFiltered.Select(x => x.CohortId);

                //Get apprentices count for cohorts.
                var apprenticesTask = _db.Value.DraftApprenticeships.Where(y => cohortIds.Contains(y.CommitmentId))
                    .GroupBy(x => x.CommitmentId).Select(z => new
                    {
                        CohortId = z.Key,
                        NumberOfApprentices = z.Count()
                    }).ToArrayAsync(cancellationToken);

                //Get latest messages for cohorts.
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

                // update CohortSummary with apprentices and messages.
                var cohorts = cohortFiltered.GroupJoin(apprentices, c => c.CohortId, a => a.CohortId,
                        (c, a) => {
                            c.NumberOfDraftApprentices = a.FirstOrDefault()?.NumberOfApprentices ?? 0;
                            return c;
                           })
                       .GroupJoin(messages, c => c.CohortId, m => m.CommitmentId,
                       (cs, message) => 
                       {
                           cs.LatestMessageFromEmployer = message?.Where(x => x.CreatedBy == 0).Select(m => new Message(m.Text, m.CreatedDateTime)).FirstOrDefault();
                           cs.LatestMessageFromProvider = message?.Where(x => x.CreatedBy == 1).Select(m => new Message(m.Text, m.CreatedDateTime)).FirstOrDefault();
                           return cs;
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