using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;

public class GetCohortSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> db, IEmailOptionalService emailService)
    : IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>
{
    public async Task<GetCohortSummaryQueryResult> Handle(GetCohortSummaryQuery request, CancellationToken cancellationToken)
    {
        var db1 = db.Value;
        var apprenticeEmailIsRequired = false;

        var parties = await db1.Cohorts
            .Select(x => new { x.Id, x.EmployerAccountId, x.ProviderId })
            .FirstOrDefaultAsync(c => c.Id == request.CohortId, cancellationToken);

        if (parties != null)
        {
            apprenticeEmailIsRequired = emailService.ApprenticeEmailIsRequiredFor(parties.EmployerAccountId, parties.ProviderId);
        }

        var result = await db1.Cohorts
            .Select(cohort => new GetCohortSummaryQueryResult
            {
                CohortId = cohort.Id,
                AccountId = cohort.EmployerAccountId,
                CohortReference = cohort.Reference,
                AccountLegalEntityId = cohort.AccountLegalEntity.Id,
                AccountLegalEntityPublicHashedId = cohort.AccountLegalEntity.PublicHashedId,
                LegalEntityName = cohort.AccountLegalEntity.Name,
                ProviderName = cohort.Provider.Name,
                TransferSenderId = cohort.TransferSenderId,
                TransferSenderName = cohort.TransferSender.Name,
                PledgeApplicationId = cohort.PledgeApplicationId,
                WithParty = cohort.WithParty,
                LatestMessageCreatedByEmployer = cohort.Messages.Where(m => m.CreatedBy == 0).OrderByDescending(m => m.CreatedDateTime).Select(m => m.Text).FirstOrDefault(),
                LatestMessageCreatedByProvider = cohort.Messages.Where(m => m.CreatedBy == 1).OrderByDescending(m => m.CreatedDateTime).Select(m => m.Text).FirstOrDefault(),
                ProviderId = cohort.ProviderId,
                LastAction = cohort.LastAction,
                LastUpdatedByEmployerEmail = cohort.LastUpdatedByEmployerEmail,
                LastUpdatedByProviderEmail = cohort.LastUpdatedByProviderEmail,
                Approvals = cohort.Approvals,
                IsApprovedByEmployer = cohort.Approvals.HasFlag(Party.Employer), //redundant
                IsApprovedByProvider = cohort.Approvals.HasFlag(Party.Provider), //redundant
                LevyStatus = cohort.AccountLegalEntity.Account.LevyStatus,
                ChangeOfPartyRequestId = cohort.ChangeOfPartyRequestId,
                TransferApprovalStatus = cohort.TransferApprovalStatus,
                ApprenticeEmailIsRequired = apprenticeEmailIsRequired
            })
            .SingleOrDefaultAsync(c => c.CohortId == request.CohortId, cancellationToken);

        if (result == null)
        {
            return null;
        }
        
        var cohortApprenticeships = await db1.DraftApprenticeships
            .Include(a => a.PriorLearning)
            .Include(a => a.FlexibleEmployment)
            .Where(a => a.CommitmentId == request.CohortId)
            .ToListAsync(cancellationToken: cancellationToken);

        result.IsCompleteForEmployer = CalculateIsCompleteForEmployer(cohortApprenticeships, apprenticeEmailIsRequired);
        result.IsCompleteForProvider = CalculateIsCompleteForProvider(cohortApprenticeships, apprenticeEmailIsRequired);

        return result;
    }

    private static bool CalculateIsCompleteForProvider(IReadOnlyCollection<ApprenticeshipBase> apprenticeships, bool apprenticeEmailIsRequired)
    {
        return CalculateIsCompleteForEmployer(apprenticeships, apprenticeEmailIsRequired)
               && apprenticeships.All(a => a.Uln != null)
               && !apprenticeships.Any(a => a.IsOnFlexiPaymentPilot.GetValueOrDefault() && (a.TrainingPrice == null || a.EndPointAssessmentPrice == null))
               && !PriorLearningStillNeedsToBeConsidered(apprenticeships);
    }

    private static bool PriorLearningStillNeedsToBeConsidered(IEnumerable<ApprenticeshipBase> apprenticeships)
    {
        return apprenticeships.Any(apprenticeship =>
        {
            if(!apprenticeship.RecognisingPriorLearningExtendedStillNeedsToBeConsidered)
            {
                return false;
            }

            return true;
        });
    }

    private static bool CalculateIsCompleteForEmployer(IReadOnlyCollection<ApprenticeshipBase> apprenticeships, bool apprenticeEmailIsRequired)
    {
        return apprenticeships.Count != 0 && !apprenticeships.Any(HasMissingData);

        bool HasMissingData(ApprenticeshipBase apprenticeship)
        {
            if (apprenticeship.FirstName == null
                || apprenticeship.LastName == null
                || apprenticeship.DateOfBirth == null
                || apprenticeship.CourseName == null
                || (apprenticeship.StartDate == null && apprenticeship.ActualStartDate == null)
                || apprenticeship.EndDate == null
                || apprenticeship.Cost == null)
            {
                return true;
            }

            if (apprenticeEmailIsRequired && apprenticeship.Email == null && apprenticeship.ContinuationOfId == null)
            {
                return true;
            }

            return apprenticeship.DeliveryModel == DeliveryModel.PortableFlexiJob
                   && (apprenticeship.FlexibleEmployment?.EmploymentEndDate == null
                       || apprenticeship.FlexibleEmployment?.EmploymentPrice == null);
        }
    }
}