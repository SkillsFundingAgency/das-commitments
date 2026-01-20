using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.ReservationsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.CocApprovals;

public class ValidateApprenticeshipForEditRequestToValidateApprenticeshipForEditCommand : IMapper<CocApprovalRequest, ValidateApprenticeshipForEditCommand>
{
    public Task<ValidateApprenticeshipForEditCommand> Map(CocApprovalRequest request)
    {
        return Task.FromResult(new ValidateApprenticeshipForEditCommand
        {
            ApprenticeshipValidationRequest = new EditApprenticeshipValidationRequest
            {
            }
        });
    }
}