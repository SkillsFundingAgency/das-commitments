using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryQueryHandler : IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IApprenticeEmailFeatureService _apprenticeEmailFeatureService;

        public GetCohortSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> db, IApprenticeEmailFeatureService apprenticeEmailFeatureService)
        {
            _db = db;
            _apprenticeEmailFeatureService = apprenticeEmailFeatureService;
        }

        public async Task<GetCohortSummaryQueryResult> Handle(GetCohortSummaryQuery request, CancellationToken cancellationToken)
        {
            var db = _db.Value;
            var apprenticeEmailIsRequired = false;

            if (_apprenticeEmailFeatureService.IsEnabled)
            {
                var parties = await db.Cohorts.Select(x=> new { x.Id, x.EmployerAccountId, x.ProviderId }).FirstOrDefaultAsync(c=>c.Id == request.CohortId, cancellationToken);
                apprenticeEmailIsRequired = await _apprenticeEmailFeatureService.ApprenticeEmailIsRequiredFor(parties.EmployerAccountId, parties.ProviderId);
            }

            var result = await (
                from c in db.Cohorts
                where c.Id == request.CohortId
                let messages = c.Messages.OrderByDescending(m => m.CreatedDateTime)
                let latestMessageCreatedByEmployer = messages.Where(m => m.CreatedBy == 0).Select(m => m.Text).FirstOrDefault()
                let latestMessageCreatedByProvider = messages.Where(m => m.CreatedBy == 1).Select(m => m.Text).FirstOrDefault()
                select new GetCohortSummaryQueryResult
                {
                    CohortId = c.Id,
                    AccountId = c.EmployerAccountId,
                    CohortReference = c.Reference,
                    AccountLegalEntityId = c.AccountLegalEntity.Id,
                    AccountLegalEntityPublicHashedId = c.AccountLegalEntity.PublicHashedId,
                    LegalEntityName = c.AccountLegalEntity.Name,
                    ProviderName = c.Provider.Name,
                    TransferSenderId = c.TransferSenderId,
                    TransferSenderName = c.TransferSender.Name,
                    WithParty = c.WithParty,
                    LatestMessageCreatedByEmployer = latestMessageCreatedByEmployer,
                    LatestMessageCreatedByProvider = latestMessageCreatedByProvider,
                    ProviderId = c.ProviderId,
                    LastAction = c.LastAction,
                    LastUpdatedByEmployerEmail = c.LastUpdatedByEmployerEmail,
                    LastUpdatedByProviderEmail = c.LastUpdatedByProviderEmail,
                    Approvals = c.Approvals,
                    IsApprovedByEmployer = c.Approvals.HasFlag(Party.Employer), //redundant
                    IsApprovedByProvider = c.Approvals.HasFlag(Party.Provider), //redundant
                    IsCompleteForEmployer = c.Apprenticeships.Any() && 
                                            !c.Apprenticeships.Any(a => a.FirstName == null || a.LastName == null || a.DateOfBirth == null || 
                                                                        a.CourseName == null || a.StartDate == null || a.EndDate == null || a.Cost == null ||
                                                                        (apprenticeEmailIsRequired && a.Email == null)),
                    LevyStatus = c.AccountLegalEntity.Account.LevyStatus,
                    ChangeOfPartyRequestId = c.ChangeOfPartyRequestId
                })
                .SingleOrDefaultAsync(cancellationToken);

            return result;
        }
    }
}