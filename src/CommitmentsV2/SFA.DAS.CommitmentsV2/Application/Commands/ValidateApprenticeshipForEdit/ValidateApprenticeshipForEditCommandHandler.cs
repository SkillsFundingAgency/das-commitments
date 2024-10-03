using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;

public class ValidateApprenticeshipForEditCommandHandler(
    IEditApprenticeshipValidationService editValidationService)
    : IRequestHandler<ValidateApprenticeshipForEditCommand, EditApprenticeshipValidationResult>
{
    public async Task<EditApprenticeshipValidationResult> Handle(ValidateApprenticeshipForEditCommand command, CancellationToken cancellationToken)
    {
        var response = await editValidationService.Validate(command.ApprenticeshipValidationRequest, cancellationToken);
            
        response?.Errors?.ThrowIfAny();

        return response;
    }
}