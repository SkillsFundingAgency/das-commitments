using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships
{
    public class GetSupportCohortSummaryHandler : IRequestHandler<GetSupportCohortSummaryQuery, GetSupportCohortSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IEmailOptionalService _emailService;
        private readonly IMapper<Apprenticeship, SupportApprenticeshipDetails> _mapper;

        public GetSupportCohortSummaryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IMapper<Apprenticeship, SupportApprenticeshipDetails> mapper, IEmailOptionalService emailService)
        {
            _db = dbContext;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<GetSupportCohortSummaryQueryResult> Handle(GetSupportCohortSummaryQuery query, CancellationToken cancellationToken)
        {
            var cohort = await _db.Value.Cohorts
                 .Include(x => x.AccountLegalEntity).Include(x => x.AccountLegalEntity.Account)
                 .Include(x => x.Provider)
                 .Include(x => x.TransferSender)
                 .Include(x => x.Apprenticeships).ThenInclude(x => x.FlexibleEmployment)
                .SingleOrDefaultAsync(c => c.Id == query.CohortId, cancellationToken);

            var apprenticeEmailIsRequired = _emailService.ApprenticeEmailIsRequiredFor(cohort.EmployerAccountId, cohort.ProviderId);

            var response = new GetSupportCohortSummaryQueryResult
            {
                CohortId = cohort.Id,
                AccountId = cohort.EmployerAccountId,
                CohortReference = cohort.Reference,
                AccountLegalEntityId = cohort.AccountLegalEntity.Id,
                AccountLegalEntityPublicHashedId = cohort.AccountLegalEntity.PublicHashedId,
                LegalEntityName = cohort.AccountLegalEntity.Name,
                ProviderName = cohort.Provider.Name,
                TransferSenderId = cohort.TransferSenderId,
                TransferSenderName = cohort?.TransferSender?.Name,
                PledgeApplicationId = cohort.PledgeApplicationId,
                WithParty = cohort.WithParty,
                LatestMessageCreatedByEmployer = cohort.Messages.OrderByDescending(m => m.CreatedDateTime).Where(m => m.CreatedBy == 0).Select(m => m.Text).FirstOrDefault(),
                LatestMessageCreatedByProvider = cohort.Messages.OrderByDescending(m => m.CreatedDateTime).Where(m => m.CreatedBy == 1).Select(m => m.Text).FirstOrDefault(),
                ProviderId = cohort.ProviderId,
                LastAction = cohort.LastAction,
                LastUpdatedByEmployerEmail = cohort.LastUpdatedByEmployerEmail,
                LastUpdatedByProviderEmail = cohort.LastUpdatedByProviderEmail,
                Approvals = cohort.Approvals,
                IsApprovedByEmployer = cohort.Approvals.HasFlag(Party.Employer), //redundant
                IsApprovedByProvider = cohort.Approvals.HasFlag(Party.Provider), //redundant

                IsCompleteForEmployer = CalculateIsCompleteForEmployer(cohort, apprenticeEmailIsRequired),
                IsCompleteForProvider = CalculateIsCompleteForProvider(cohort, apprenticeEmailIsRequired),

                LevyStatus = cohort.AccountLegalEntity.Account.LevyStatus,
                ChangeOfPartyRequestId = cohort.ChangeOfPartyRequestId,
                TransferApprovalStatus = cohort.TransferApprovalStatus,
                ApprenticeEmailIsRequired = apprenticeEmailIsRequired,
                EditStatus = cohort.EditStatus
            };

            return response;
        }

        private static bool CalculateIsCompleteForProvider(Models.Cohort c, bool apprenticeEmailIsRequired)
        {
            return CalculateIsCompleteForEmployer(c, apprenticeEmailIsRequired)
                && !c.Apprenticeships.Any(a => a.Uln == null);
        }

        private static bool CalculateIsCompleteForEmployer(Models.Cohort c, bool apprenticeEmailIsRequired)
        {
            return c.Apprenticeships.Any() && !c.Apprenticeships.Any(HasMissingData);

            bool HasMissingData(Models.ApprenticeshipBase a)
            {
                if (a.FirstName == null
                    || a.LastName == null
                    || a.DateOfBirth == null
                    || a.CourseName == null
                    || a.StartDate == null
                    || a.EndDate == null
                    || a.Cost == null)
                {
                    return true;
                }

                if (apprenticeEmailIsRequired && a.Email == null && a.ContinuationOfId == null)
                {
                    return true;
                }

                if (a.DeliveryModel == DeliveryModel.PortableFlexiJob
                    && (a.FlexibleEmployment?.EmploymentEndDate == null
                    || a.FlexibleEmployment?.EmploymentPrice == null))
                {
                    return true;
                }

                return false;
            }
        }
    }
}