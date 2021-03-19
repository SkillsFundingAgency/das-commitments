using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit
{
    public class ValidateApprenticeshipForEditCommandHandler : AsyncRequestHandler<ValidateApprenticeshipForEditCommand>
    {
        private readonly IEditApprenticeshipValidationService _editValidationService;
        private readonly IModelMapper _modelMapper;

        public ValidateApprenticeshipForEditCommandHandler(IEditApprenticeshipValidationService editValidationService, IModelMapper modelMapper)
        {
            _editValidationService = editValidationService;
            _modelMapper = modelMapper;
        }

        protected async override Task Handle(ValidateApprenticeshipForEditCommand command, CancellationToken cancellationToken)
        {
            var request = await _modelMapper.Map<Domain.Entities.EditApprenticeshipValidation.EditApprenticeshipValidationRequest>(command);
            var response = await _editValidationService.Validate(request, CancellationToken.None);
            //var response = await _editValidationService.Validate(new Domain.Entities.EditApprenticeshipValidation.EditApprenticeshipValidationRequest
            //{
            //    ProviderId = request.ProviderId,
            //    EmployerAccountId = request.EmployerAccountId,
            //    ApprenticeshipId = request.ApprenticeshipId,
            //    FirstName = request.FirstName,
            //    LastName = request.LastName,
            //    DateOfBirth = request.DateOfBirth,
            //    ULN = request.ULN,
            //    Cost = request.Cost,
            //    EmployerReference = request.EmployerReference,
            //    StartDate = request.StartDate,
            //    EndDate = request.EndDate,
            //    TrainingCode = request.TrainingCode
            //}, CancellationToken.None);

            response.Errors.ThrowIfAny();
        }
    }
}
