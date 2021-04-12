using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Domain.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship
{
    public class EditApprenticeshipCommandHandler : IRequestHandler<EditApprenticeshipCommand, EditApprenticeshipResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IEditApprenticeshipValidationService _editValidationService;
        private readonly IModelMapper _modelMapper;
        private readonly IAuthenticationService _authnticationService;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly ILogger<EditApprenticeshipCommandHandler> _logger;

        public EditApprenticeshipCommandHandler(IEditApprenticeshipValidationService editValidationService, Lazy<ProviderCommitmentsDbContext> dbContext, IModelMapper mapper, IAuthenticationService authenticationService, ICurrentDateTime currentDateTime, ILogger<EditApprenticeshipCommandHandler> logger)
        {
            _editValidationService = editValidationService;
            _dbContext = dbContext;
            _modelMapper = mapper;
            _authnticationService = authenticationService;
            _currentDateTime = currentDateTime;
            _logger = logger;
        }

        public async Task<EditApprenticeshipResponse> Handle(EditApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            var party = Party.Employer;  //_authnticationService.GetUserParty();
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);

            await Validate(command, apprenticeship, party, cancellationToken);

            CreateImmedidateUpdate(command, party, apprenticeship);

            var immediateUpdateCreated = CreateIntermediateUpdate(command, party, apprenticeship);

            return new EditApprenticeshipResponse { ApprenticeshipId = command.ApprenticeshipId, NeedReapproval = immediateUpdateCreated };
        }

        private bool CreateImmedidateUpdate(EditApprenticeshipCommand command, Party party, Apprenticeship apprenticeship)
        {
            bool immediateUpdate = false;
            if (command.EmployerReferenceUpdateRequired(apprenticeship, party))
            {
                apprenticeship.UpdateEmployerReference(command.EmployerReference, party, command.UserInfo);
                immediateUpdate = true;
            }
            else
            {
                if (command.ProviderReferenceUpdateRequired(apprenticeship, party))
                {
                    apprenticeship.UpdateProviderReference(command.ProviderReference, party, command.UserInfo);
                    immediateUpdate = true;
                }

                if (command.ULNUpdateRequired(apprenticeship, party))
                {
                    apprenticeship.UpdateULN(command.ULN, party, _currentDateTime.UtcNow, command.UserInfo);
                    immediateUpdate = true;
                }
            }

            return immediateUpdate;
        }

        private bool CreateIntermediateUpdate(EditApprenticeshipCommand command, Party party, Apprenticeship apprenticeship)
        {
            var apprenticeshipUpdate = command.MapToApprenticeshipUpdate(apprenticeship, party, _currentDateTime.UtcNow);

            if (apprenticeshipUpdate.ApprenticeshipUpdateRequired())
            {
                apprenticeship.CreateApprenticeshipUpdate(apprenticeshipUpdate, party);
                return true;
            }

            return false;
        }

        private async Task Validate(EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party, CancellationToken cancellationToken)
        {
            CheckPartyIsValid(party);

            var validateRequest = await _modelMapper.Map<EditApprenticeshipValidationRequest>(command);
            var validationResult = await _editValidationService.Validate(validateRequest, cancellationToken);
            if (validationResult?.Errors?.Count > 0)
            {
                string messages = string.Empty;
                validationResult.Errors.ForEach(x => messages += (x.PropertyName + ":" + x.ErrorMessage));

                _logger.LogError("Invalid operation for edit - the following error/s occured :" + messages);
                // This shouldn't get triggered as these checks should already been passed.
                // But in case someone is calling the EditApprenticeship endpoint directly
                throw new InvalidOperationException("The operation is not allowed for the current state of the object");
            }

            if (apprenticeship.ApprenticeshipUpdate.Count > 0)
            {
                throw new InvalidOperationException("Unable to create an ApprenticeshipUpdate for an Apprenticeship with a pending update");
            }
        }

        private void CheckPartyIsValid(Party party)
        {
            if (party != Party.Employer)
            {
                throw new DomainException(nameof(party), $"Only employers are allowed to edit the records");
            }
        }
    }
}
