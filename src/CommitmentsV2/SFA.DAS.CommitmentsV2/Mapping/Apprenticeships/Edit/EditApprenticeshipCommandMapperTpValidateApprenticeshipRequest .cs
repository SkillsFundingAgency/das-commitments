using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.Edit
{
    public class EditApprenticeshipCommandMapperTpValidateApprenticeshipRequest : IMapper<EditApprenticeshipCommand, EditApprenticeshipValidationRequest>
    {
        public Task<EditApprenticeshipValidationRequest> Map(EditApprenticeshipCommand request)
        {
            return Task.FromResult(new EditApprenticeshipValidationRequest
            {
                ProviderId = request.ProviderId,
                EmployerAccountId = request.AccountId,
                ApprenticeshipId = request.ApprenticeshipId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                ULN = request.ULN,
                Cost = request.Cost,
                EmployerReference = request.EmployerReference,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CourseCode = request.CourseCode
            });
        }
    }
}
