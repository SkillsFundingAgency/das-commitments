using SFA.DAS.CommitmentsV2.Types;
using HttpResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses;
using CommandResponse = SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers;

public class GetDraftApprenticeshipResponseToGetDraftApprenticeshipResponseMapper : IOldMapper<CommandResponse.GetDraftApprenticeshipQueryResult, HttpResponse.GetDraftApprenticeshipResponse>
{
    public Task<HttpResponse.GetDraftApprenticeshipResponse> Map(CommandResponse.GetDraftApprenticeshipQueryResult source)
    {
        return Task.FromResult(new HttpResponse.GetDraftApprenticeshipResponse
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            DateOfBirth = source.DateOfBirth,
            Uln = source.Uln,
            CourseCode = source.CourseCode,
            TrainingCourseVersion = source.TrainingCourseVersion,
            TrainingCourseName = source.TrainingCourseName,
            TrainingCourseOption = source.TrainingCourseOption,
            TrainingCourseVersionConfirmed = source.TrainingCourseVersionConfirmed,
            StandardUId = source.StandardUId,
            DeliveryModel = source.DeliveryModel ?? DeliveryModel.Regular,
            Cost = source.Cost,
            TrainingPrice = source.TrainingPrice,
            EndPointAssessmentPrice = source.EndPointAssessmentPrice,
            StartDate = source.StartDate,
            ActualStartDate = source.ActualStartDate,
            EndDate = source.EndDate,
            Reference = source.Reference,
            EmployerReference = source.EmployerReference,
            ProviderReference = source.ProviderReference,
            ReservationId = source.ReservationId,
            IsContinuation = source.IsContinuation,
            ContinuationOfId = source.ContinuationOfId,
            OriginalStartDate = source.OriginalStartDate,
            HasStandardOptions = source.HasStandardOptions,
            EmploymentPrice = source.EmploymentPrice,
            EmploymentEndDate = source.EmploymentEndDate,
            RecognisePriorLearning = source.RecognisePriorLearning,
            DurationReducedBy = source.DurationReducedBy,
            PriceReducedBy = source.PriceReducedBy,
            RecognisingPriorLearningExtendedStillNeedsToBeConsidered = source.RecognisingPriorLearningExtendedStillNeedsToBeConsidered,
            EmailAddressConfirmed = source.EmailAddressConfirmed,
            DurationReducedByHours = source.DurationReducedByHours,
            IsDurationReducedByRpl = source.IsDurationReducedByRpl,
            TrainingTotalHours = source.TrainingTotalHours,
            EmployerHasEditedCost = source.EmployerHasEditedCost,
            LearnerDataId = source.LearnerDataId,
            HasLearnerDataChanges = source.HasLearnerDataChanges,
            LastLearnerDataSync = source.LastLearnerDataSync
        });
    }
}