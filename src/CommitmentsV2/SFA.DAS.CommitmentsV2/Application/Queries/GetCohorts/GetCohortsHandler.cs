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
                var query =
                    from c in _db.Value.Cohorts
                    where c.EmployerAccountId == command.AccountId &&
                          (c.EditStatus != EditStatus.Both ||
                           (c.TransferSenderId != null && c.TransferApprovalStatus != TransferApprovalStatus.Approved))
                    let messages = c.Messages.OrderByDescending(m => m.CreatedDateTime)
                    let latestMessageCreatedByEmployer = messages.Where(m => m.CreatedBy == 0)
                        .Select(m => new Message(m.Text, m.CreatedDateTime)).FirstOrDefault()
                    let latestMessageCreatedByProvider = messages.Where(m => m.CreatedBy == 1)
                        .Select(m => new Message(m.Text, m.CreatedDateTime)).FirstOrDefault()

                    select new CohortSummary
                    {
                        AccountId = c.EmployerAccountId,
                        LegalEntityName = c.LegalEntityName,
                        ProviderId = c.ProviderId.Value,
                        ProviderName = c.ProviderName,
                        CohortId = c.Id,
                        NumberOfDraftApprentices = c.Apprenticeships.Count,
                        LatestMessageFromEmployer = latestMessageCreatedByEmployer,
                        LatestMessageFromProvider = latestMessageCreatedByProvider,
                        IsDraft = c.LastAction == LastAction.None,
                        WithParty = c.WithParty,
                        CreatedOn = c.CreatedOn.Value
                    };

                var cohorts = await query.ToArrayAsync(cancellationToken);

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