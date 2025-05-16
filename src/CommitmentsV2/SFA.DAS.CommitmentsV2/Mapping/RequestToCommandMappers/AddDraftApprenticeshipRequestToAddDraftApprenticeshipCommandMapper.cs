using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

public class AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper : IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>
{
    public Task<AddDraftApprenticeshipCommand> Map(AddDraftApprenticeshipRequest source)
    {
        return Task.FromResult(new AddDraftApprenticeshipCommand
        {
            RequestingParty = source.RequestingParty,
            UserId = source.UserId,
            ProviderId = source.ProviderId,
            CourseCode = source.CourseCode,
            DeliveryModel = source.DeliveryModel,
            Cost = source.Cost,
            StartDate = source.StartDate,
            ActualStartDate = source.ActualStartDate,
            EndDate = source.EndDate,
            OriginatorReference = source.OriginatorReference,
            ReservationId = source.ReservationId,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            DateOfBirth = source.DateOfBirth,
            Uln = source.Uln,
            EmploymentEndDate = source.EmploymentEndDate,
            EmploymentPrice = source.EmploymentPrice,
            UserInfo = source.UserInfo,
            IgnoreStartDateOverlap = source.IgnoreStartDateOverlap,
            IsOnFlexiPaymentPilot = source.IsOnFlexiPaymentPilot,
            TrainingPrice = source.TrainingPrice,
            EndPointAssessmentPrice = source.EndPointAssessmentPrice,
            LearnerDataId = source.LearnerDataId
        });
    }
}