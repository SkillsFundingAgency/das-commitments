using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryQueryHandler : IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IEncodingService _encodingService;

        public GetCohortSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> db, IEncodingService encodingService)
        {
            _db = db;
            _encodingService = encodingService;
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
                    AccountLegalEntityPublicHashedId = c.AccountLegalEntityPublicHashedId,
                    LegalEntityName = c.LegalEntityName,
                    ProviderName = c.ProviderName,
                    TransferSenderId = c.TransferSenderId,
                    WithParty = c.WithParty,
                    LatestMessageCreatedByEmployer = latestMessageCreatedByEmployer,
                    LatestMessageCreatedByProvider = latestMessageCreatedByProvider,
                    ProviderId = c.ProviderId,
                    LastAction = c.LastAction,
                    LastUpdatedByEmployerEmail = c.LastUpdatedByEmployerEmail,
                    LastUpdatedByProviderEmail = c.LastUpdatedByProviderEmail,
 	 				IsApprovedByEmployer = c.EditStatus == EditStatus.Both || (c.Apprenticeships.Any() && c.Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.EmployerAgreed || a.AgreementStatus == AgreementStatus.BothAgreed)),
                    IsApprovedByProvider = c.EditStatus == EditStatus.Both || (c.Apprenticeships.Any() && c.Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.ProviderAgreed || a.AgreementStatus == AgreementStatus.BothAgreed))
                })
                .SingleOrDefaultAsync(cancellationToken);

            result.AccountLegalEntityId = _encodingService.Decode(result.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);

            return result;
        }
    }
}