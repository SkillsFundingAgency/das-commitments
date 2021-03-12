using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit
{
    public class ValidateApprenticeshipForEditCommandHandler : AsyncRequestHandler<ValidateApprenticeshipForEditCommand>
    {
        private IEditApprenticeshipValidationService _editValidationService;

        public ValidateApprenticeshipForEditCommandHandler(IEditApprenticeshipValidationService editValidationService)
        {
            _editValidationService = editValidationService;
        }

        protected async override Task Handle(ValidateApprenticeshipForEditCommand request, CancellationToken cancellationToken)
        {
            var response = await _editValidationService.Validate(new Domain.Entities.EditApprenticeshipValidation.EditApprenticeshipValidationRequest
            {
                ProviderId = request.ProviderId,
                EmployerAccountId = request.EmployerAccountId,
                ApprenticeshipId = request.ApprenticeshipId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                ULN = request.ULN,
                Cost = request.Cost,
                EmployerReference = request.EmployerReference,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TrainingCode = request.TrainingCode
            }, CancellationToken.None);

            response.Errors.ThrowIfAny();
        }
    }
}
