using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryQueryHandler : IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetCohortSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetCohortSummaryQueryResult> Handle(GetCohortSummaryQuery request, CancellationToken cancellationToken)
        {
            var result = await (
                from c in _db.Value.Cohorts
                where c.Id == request.CohortId
                let messages = c.Messages.OrderByDescending(m => m.CreatedDateTime)
                let latestMessageCreatedByEmployer = messages.Where(m => m.CreatedBy == 0).Select(m => m.Text).FirstOrDefault()
                let latestMessageCreatedByProvider = messages.Where(m => m.CreatedBy == 1).Select(m => m.Text).FirstOrDefault()
                select new GetCohortSummaryQueryResult
                {
                    CohortId = c.Id,
                    AccountId = c.EmployerAccountId,
                    CohortReference = c.Reference,
                    AccountLegalEntityId = c.AccountLegalEntityId.Value,
                    AccountLegalEntityPublicHashedId = c.AccountLegalEntityPublicHashedId,
                    LegalEntityName = c.AccountLegalEntity.Name,
                    ProviderName = c.Provider.Name,
                    TransferSenderId = c.TransferSenderId,
                    TransferSenderName = c.TransferSenderName,
                    WithParty = c.WithParty,
                    LatestMessageCreatedByEmployer = latestMessageCreatedByEmployer,
                    LatestMessageCreatedByProvider = latestMessageCreatedByProvider,
                    ProviderId = c.ProviderId,
                    LastAction = c.LastAction,
                    LastUpdatedByEmployerEmail = c.LastUpdatedByEmployerEmail,
                    LastUpdatedByProviderEmail = c.LastUpdatedByProviderEmail,
 	 				IsApprovedByEmployer = c.EditStatus == EditStatus.Both || (c.Apprenticeships.Any() && c.Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.EmployerAgreed || a.AgreementStatus == AgreementStatus.BothAgreed)),
                    IsApprovedByProvider = c.EditStatus == EditStatus.Both || (c.Apprenticeships.Any() && c.Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.ProviderAgreed || a.AgreementStatus == AgreementStatus.BothAgreed)),
                    IsCompleteForEmployer = c.Apprenticeships.Any() && !c.Apprenticeships.Any(a => a.FirstName == null || a.LastName == null || a.DateOfBirth == null || a.CourseName == null || a.StartDate == null || a.EndDate == null || a.Cost == null)
                })
                .SingleOrDefaultAsync(cancellationToken);

            return result;
        }
    }
}