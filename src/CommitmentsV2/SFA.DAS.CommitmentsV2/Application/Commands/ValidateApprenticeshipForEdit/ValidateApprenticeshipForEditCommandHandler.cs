using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit
{
    public class ValidateApprenticeshipForEditCommandHandler : IRequestHandler<ValidateApprenticeshipForEditCommand, EditApprenticeshipValidationResult>
    {
        private readonly IEditApprenticeshipValidationService _editValidationService;

        public ValidateApprenticeshipForEditCommandHandler(IEditApprenticeshipValidationService editValidationService)
        {
            _editValidationService = editValidationService;
        }

        public async Task<EditApprenticeshipValidationResult> Handle(ValidateApprenticeshipForEditCommand command, CancellationToken cancellationToken)
        {
            var response = await _editValidationService.Validate(command.ApprenticeshipValidationRequest, cancellationToken);
            
            response?.Errors?.ThrowIfAny();

            return response;
        }
    }
}
