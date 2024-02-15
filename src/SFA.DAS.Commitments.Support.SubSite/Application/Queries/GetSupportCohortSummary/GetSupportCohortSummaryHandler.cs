using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportCohortSummary
{
    public class GetSupportCohortSummaryHandler : IRequestHandler<GetSupportCohortSummaryQuery, GetSupportCohortSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IEmailOptionalService _emailService;
        public GetSupportCohortSummaryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IEmailOptionalService emailService)
        {
            _db = dbContext;
            _emailService = emailService;
        }

        public async Task<GetSupportCohortSummaryQueryResult> Handle(GetSupportCohortSummaryQuery query, CancellationToken cancellationToken)
        {
            var cohort = await _db.Value.Cohorts
                 .Include(x => x.AccountLegalEntity).Include(x => x.AccountLegalEntity.Account)
                 .Include(x => x.Provider)
                 .Include(x => x.TransferSender)
                 .Include(x => x.Apprenticeships).ThenInclude(x => x.FlexibleEmployment)
                 .Include(x => x.Apprenticeships).ThenInclude(x => x.PriorLearning)
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

        private static bool CalculateIsCompleteForProvider(CommitmentsV2.Models.Cohort c, bool apprenticeEmailIsRequired)
        {
            return CalculateIsCompleteForEmployer(c, apprenticeEmailIsRequired)
                   && !c.Apprenticeships.Any(a => a.Uln == null)
                   && !c.Apprenticeships.Any(a => a.RecognisingPriorLearningStillNeedsToBeConsidered);
        }

        private static bool CalculateIsCompleteForEmployer(CommitmentsV2.Models.Cohort c, bool apprenticeEmailIsRequired)
        {
            return c.Apprenticeships.Any() && !c.Apprenticeships.Any(HasMissingData);

            bool HasMissingData(CommitmentsV2.Models.ApprenticeshipBase a)
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