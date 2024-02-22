using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;

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
            var cohortFiltered = await (from cohort in _db.Value.Cohorts
                join a in _db.Value.Accounts on cohort.TransferSenderId equals a.Id into account
                from transferSender in account.DefaultIfEmpty()
                where cohort.EmployerAccountId == (command.AccountId ?? cohort.EmployerAccountId) && cohort.ProviderId == (command.ProviderId ?? cohort.ProviderId) &&
                      (cohort.EditStatus != EditStatus.Both ||
                       (cohort.TransferSenderId != null && cohort.TransferApprovalStatus == TransferApprovalStatus.Pending))
                select new CohortSummary
                {
                    AccountId = cohort.EmployerAccountId,
                    LegalEntityName = cohort.AccountLegalEntity.Name,
                    AccountLegalEntityPublicHashedId = cohort.AccountLegalEntity.PublicHashedId,
                    ProviderId = cohort.ProviderId,
                    ProviderName = cohort.Provider.Name,
                    CohortId = cohort.Id,
                    IsDraft = cohort.IsDraft,
                    WithParty = cohort.WithParty,
                    CreatedOn = cohort.CreatedOn.Value,
                    TransferSenderName = transferSender.Name,
                    TransferSenderId = cohort.TransferSenderId,
                    IsLinkedToChangeOfPartyRequest = cohort.IsLinkedToChangeOfPartyRequest,
                    CommitmentStatus = cohort.CommitmentStatus,
                    PledgeApplicationId = cohort.PledgeApplicationId
                }).ToArrayAsync(cancellationToken);

            var cohortIds = cohortFiltered.Select(x => x.CohortId);

            //Get apprentices count for cohorts.
            var apprentices = await _db.Value.DraftApprenticeships.Where(y => cohortIds.Contains(y.CommitmentId))
                .GroupBy(x => x.CommitmentId).Select(z => new
                {
                    CohortId = z.Key,
                    NumberOfApprentices = z.Count()
                }).ToArrayAsync(cancellationToken);

            //Get latest messages for cohorts.
            var filteredMessages = _db.Value.Messages.Where(y => cohortIds.Contains(y.CommitmentId))
                .GroupBy(message => new { message.CreatedBy, message.CommitmentId })
                .Select(z => z.Max(x => x.Id));
            var messages = await _db.Value.Messages.Where(m => filteredMessages.Contains(m.Id)).Select(message => new
            {
                message.CommitmentId,
                message.CreatedBy,
                message.CreatedDateTime,
                message.Text,
            }).ToArrayAsync(cancellationToken);

            // update CohortSummary with apprentices and messages.
            var cohorts = cohortFiltered.GroupJoin(apprentices, c => c.CohortId, a => a.CohortId,
                    (cohortSummary, a) => {
                        cohortSummary.NumberOfDraftApprentices = a.FirstOrDefault()?.NumberOfApprentices ?? 0;
                        return cohortSummary;
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "{message}", exception.Message);
            throw;
        }
    }
}