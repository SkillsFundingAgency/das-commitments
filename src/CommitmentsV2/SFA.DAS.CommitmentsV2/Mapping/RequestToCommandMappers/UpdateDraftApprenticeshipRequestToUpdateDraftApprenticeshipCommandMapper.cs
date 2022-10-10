using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers
{
    public class UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapper : IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>
    {
        public Task<UpdateDraftApprenticeshipCommand>  Map(UpdateDraftApprenticeshipRequest source)
        {
            return Task.FromResult(new UpdateDraftApprenticeshipCommand
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                DateOfBirth = source.DateOfBirth,
                Uln = source.Uln,
                CourseCode = source.CourseCode,
                CourseOption = source.CourseOption,
                DeliveryModel = source.DeliveryModel,
                EmploymentPrice = source.EmploymentPrice,
                Cost = source.Cost,
                StartDate = source.StartDate,
                ActualStartDate = source.ActualStartDate,
                EmploymentEndDate = source.EmploymentEndDate,
                EndDate = source.EndDate,
                Reference = source.Reference,
                ReservationId = source.ReservationId,
                UserInfo = source.UserInfo,
                IgnoreStartDateOverlap = source.IgnoreStartDateOverlap,
                IsOnFlexiPaymentPilot = source.IsOnFlexiPaymentPilot,
                IsProviderOnFlexiPaymentPilot = source.IsProviderOnFlexiPaymentPilot,
            });
        }
    }
}