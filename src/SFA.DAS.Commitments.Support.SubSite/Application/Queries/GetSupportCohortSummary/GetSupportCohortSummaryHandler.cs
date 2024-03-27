using System.Threading;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportCohortSummary;

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
            .Include(x => x.Apprenticeships).ThenInclude(x => x.PriorLearning).Include(cohort => cohort.Messages)
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
            TransferSenderName = cohort.TransferSender?.Name,
            PledgeApplicationId = cohort.PledgeApplicationId,
            WithParty = cohort.WithParty,
            
            LatestMessageCreatedByEmployer = cohort.Messages
                .Where(m => m.CreatedBy == 0)
                .OrderByDescending(m => m.CreatedDateTime)
                .Select(m => m.Text)
                .FirstOrDefault(),
            
            LatestMessageCreatedByProvider = cohort.Messages
                .Where(m => m.CreatedBy == 1)
                .OrderByDescending(m => m.CreatedDateTime)
                .Select(m => m.Text)
                .FirstOrDefault(),
            
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
               && c.Apprenticeships.All(a => a.Uln != null) && !c.Apprenticeships.Any(a => a.RecognisingPriorLearningStillNeedsToBeConsidered);
    }

    private static bool CalculateIsCompleteForEmployer(CommitmentsV2.Models.Cohort cohort, bool apprenticeEmailIsRequired)
    {
        return cohort.Apprenticeships.Any() && !cohort.Apprenticeships.Any(HasMissingData);

        bool HasMissingData(CommitmentsV2.Models.ApprenticeshipBase apprenticeshipBase)
        {
            if (apprenticeshipBase.FirstName == null
                || apprenticeshipBase.LastName == null
                || apprenticeshipBase.DateOfBirth == null
                || apprenticeshipBase.CourseName == null
                || apprenticeshipBase.StartDate == null
                || apprenticeshipBase.EndDate == null
                || apprenticeshipBase.Cost == null)
            {
                return true;
            }

            if (apprenticeEmailIsRequired && apprenticeshipBase.Email == null && apprenticeshipBase.ContinuationOfId == null)
            {
                return true;
            }

            return apprenticeshipBase.DeliveryModel == DeliveryModel.PortableFlexiJob
                   && (apprenticeshipBase.FlexibleEmployment?.EmploymentEndDate == null
                       || apprenticeshipBase.FlexibleEmployment?.EmploymentPrice == null);
        }
    }
}