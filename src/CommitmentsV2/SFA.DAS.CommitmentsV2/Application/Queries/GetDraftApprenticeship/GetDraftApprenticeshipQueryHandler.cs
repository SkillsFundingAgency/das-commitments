using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;

public class GetDraftApprenticeshipQueryHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IAuthenticationService authenticationService)
    : IRequestHandler<GetDraftApprenticeshipQuery, GetDraftApprenticeshipQueryResult>
{
    public async Task<GetDraftApprenticeshipQueryResult> Handle(GetDraftApprenticeshipQuery request, CancellationToken cancellationToken)
    {
        var requestingParty = authenticationService.GetUserParty();

        var query = dbContext.Value.DraftApprenticeships
            .Include(x => x.PriorLearning)
            .Where(x => x.Id == request.DraftApprenticeshipId && x.CommitmentId == request.CohortId);

        var result = await query.Select(draft => new GetDraftApprenticeshipQueryResult
        {
            CourseCode = draft.CourseCode,
            TrainingCourseVersion = draft.TrainingCourseVersion,
            TrainingCourseVersionConfirmed = draft.TrainingCourseVersionConfirmed,
            TrainingCourseName = draft.CourseName,
            TrainingCourseOption = draft.TrainingCourseOption,
            StandardUId = draft.StandardUId,
            DeliveryModel = draft.DeliveryModel,
            StartDate = draft.StartDate,
            ActualStartDate = draft.ActualStartDate,
            Id = draft.Id,
            Cost = (int?)draft.Cost,
            TrainingPrice = draft.TrainingPrice,
            EndPointAssessmentPrice = draft.EndPointAssessmentPrice,
            DateOfBirth = draft.DateOfBirth,
            EndDate = draft.EndDate,
            FirstName = draft.FirstName,
            LastName = draft.LastName,
            Email = draft.Email,
            Reference = requestingParty == Party.Provider ? draft.ProviderRef : draft.EmployerRef,
            EmployerReference = draft.EmployerRef,
            ProviderReference = draft.ProviderRef,
            ReservationId = draft.ReservationId,
            Uln = draft.Uln,
            IsContinuation = draft.ContinuationOfId.HasValue,
            ContinuationOfId = draft.ContinuationOfId,
            OriginalStartDate = draft.OriginalStartDate,
            HasStandardOptions = !string.IsNullOrEmpty(draft.StandardUId) && dbContext.Value.StandardOptions.Any(c => c.StandardUId.Equals(draft.StandardUId)),
            EmploymentEndDate = draft.FlexibleEmployment != null ? draft.FlexibleEmployment.EmploymentEndDate : null,
            EmploymentPrice = draft.FlexibleEmployment != null ? draft.FlexibleEmployment.EmploymentPrice : null,
            RecognisePriorLearning = draft.RecognisePriorLearning,
            DurationReducedBy = draft.PriorLearning != null ? draft.PriorLearning.DurationReducedBy : null,
            PriceReducedBy = draft.PriorLearning != null ? draft.PriorLearning.PriceReducedBy : null,
            DurationReducedByHours = draft.PriorLearning != null ? draft.PriorLearning.DurationReducedByHours : null,
            IsDurationReducedByRpl = draft.PriorLearning != null ? draft.PriorLearning.IsDurationReducedByRpl : null,
            TrainingTotalHours = draft.PriorLearning != null ? draft.TrainingTotalHours : null,
            RecognisingPriorLearningExtendedStillNeedsToBeConsidered = draft.RecognisingPriorLearningExtendedStillNeedsToBeConsidered,
            EmailAddressConfirmed = draft.EmailAddressConfirmed,
            EmployerHasEditedCost = draft.EmployerHasEditedCost,
            LearnerDataId = draft.LearnerDataId
        }).SingleOrDefaultAsync(cancellationToken);

        return result;
    }
}
