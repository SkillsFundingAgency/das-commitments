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
        private readonly IEmailOptionalService _emailService;

        public GetCohortSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> db, IEmailOptionalService emailService)
            => (_db, _emailService) = (db, emailService);

        public async Task<GetCohortSummaryQueryResult> Handle(GetCohortSummaryQuery request, CancellationToken cancellationToken)
        {
            var db = _db.Value;
            var apprenticeEmailIsRequired = false;

            var parties = await db.Cohorts
                .Select(x => new {x.Id, x.EmployerAccountId, x.ProviderId})
                .FirstOrDefaultAsync(c => c.Id == request.CohortId, cancellationToken);
          
            if (parties != null)
            {
                apprenticeEmailIsRequired = _emailService.ApprenticeEmailIsRequiredFor(parties.EmployerAccountId, parties.ProviderId);
            }

            var result = await db.Cohorts.Select(c => new GetCohortSummaryQueryResult
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
                    PledgeApplicationId = c.PledgeApplicationId,
                    WithParty = c.WithParty,
                    LatestMessageCreatedByEmployer = c.Messages.OrderByDescending(m => m.CreatedDateTime).Where(m => m.CreatedBy == 0).Select(m => m.Text).FirstOrDefault(),
                    LatestMessageCreatedByProvider = c.Messages.OrderByDescending(m => m.CreatedDateTime).Where(m => m.CreatedBy == 1).Select(m => m.Text).FirstOrDefault(),
                    ProviderId = c.ProviderId,
                    LastAction = c.LastAction,
                    LastUpdatedByEmployerEmail = c.LastUpdatedByEmployerEmail,
                    LastUpdatedByProviderEmail = c.LastUpdatedByProviderEmail,
                    Approvals = c.Approvals,
                    IsApprovedByEmployer = c.Approvals.HasFlag(Party.Employer), //redundant
                    IsApprovedByProvider = c.Approvals.HasFlag(Party.Provider), //redundant
                    IsCompleteForEmployer = c.Apprenticeships.Any() &&
                                            !c.Apprenticeships.Any(a =>
                                                a.FirstName == null || a.LastName == null || a.DateOfBirth == null ||
                                                a.CourseName == null || a.StartDate == null || a.EndDate == null ||
                                                a.Cost == null ||
                                                (apprenticeEmailIsRequired && a.Email == null &&
                                                 a.ContinuationOfId == null)),
                    IsCompleteForProvider = c.Apprenticeships.Any() &&
                                            !c.Apprenticeships.Any(a =>
                                                a.FirstName == null || a.LastName == null || a.DateOfBirth == null ||
                                                a.CourseName == null || a.StartDate == null || a.EndDate == null ||
                                                a.Cost == null ||
                                                a.Uln == null ||
                                                (apprenticeEmailIsRequired && a.Email == null &&
                                                 a.ContinuationOfId == null)),
                    LevyStatus = c.AccountLegalEntity.Account.LevyStatus,
                    ChangeOfPartyRequestId = c.ChangeOfPartyRequestId,
                    TransferApprovalStatus = c.TransferApprovalStatus
                })
                .SingleOrDefaultAsync(c => c.CohortId == request.CohortId, cancellationToken);

            return result;
        }
    }
}