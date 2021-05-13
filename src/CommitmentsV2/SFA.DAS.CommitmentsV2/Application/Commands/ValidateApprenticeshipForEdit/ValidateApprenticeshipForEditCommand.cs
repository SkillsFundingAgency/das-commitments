using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit
{
    public class ValidateApprenticeshipForEditCommand : IRequest<EditApprenticeshipValidationResult>
    {
        public EditApprenticeshipValidationRequest ApprenticeshipValidationRequest { get; set; }
    }
}
