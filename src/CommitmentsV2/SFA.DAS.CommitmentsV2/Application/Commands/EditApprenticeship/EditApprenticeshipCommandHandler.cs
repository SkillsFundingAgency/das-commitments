﻿using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
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
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship
{
    public class EditApprenticeshipCommandHandler : IRequestHandler<EditApprenticeshipCommand, EditApprenticeshipResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IEditApprenticeshipValidationService _editValidationService;
        private readonly IAuthenticationService _authnticationService;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly ILogger<EditApprenticeshipCommandHandler> _logger;
        private readonly IMediator _mediator;

        public EditApprenticeshipCommandHandler(IEditApprenticeshipValidationService editValidationService, Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService, ICurrentDateTime currentDateTime,  IMediator mediator, ILogger<EditApprenticeshipCommandHandler> logger)
        {
            _editValidationService = editValidationService;
            _dbContext = dbContext;
            _authnticationService = authenticationService;
            _currentDateTime = currentDateTime;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<EditApprenticeshipResponse> Handle(EditApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            //TODO : Changes this back to getuserparty
            var party = _authnticationService.GetUserParty();
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.EditApprenticeshipRequest.ApprenticeshipId, cancellationToken);

            await Validate(command, apprenticeship, party, cancellationToken);

            CreateImmedidateUpdate(command, party, apprenticeship);

            var immediateUpdateCreated = CreateIntermediateUpdate(command, party, apprenticeship);

            return new EditApprenticeshipResponse { ApprenticeshipId = command.EditApprenticeshipRequest.ApprenticeshipId, NeedReapproval = immediateUpdateCreated };
        }

        private bool CreateImmedidateUpdate(EditApprenticeshipCommand command, Party party, Apprenticeship apprenticeship)
        {
            bool immediateUpdate = false;
            if (command.EmployerReferenceUpdateRequired(apprenticeship, party))
            {
                apprenticeship.UpdateEmployerReference(command.EditApprenticeshipRequest.EmployerReference, party, command.EditApprenticeshipRequest.UserInfo);
                immediateUpdate = true;
            }
            else
            {
                if (command.ProviderReferenceUpdateRequired(apprenticeship, party))
                {
                    apprenticeship.UpdateProviderReference(command.EditApprenticeshipRequest.ProviderReference, party, command.EditApprenticeshipRequest.UserInfo);
                    immediateUpdate = true;
                }

                if (command.ULNUpdateRequired(apprenticeship, party))
                {
                    apprenticeship.UpdateULN(command.EditApprenticeshipRequest.ULN, party, _currentDateTime.UtcNow, command.EditApprenticeshipRequest.UserInfo);
                    immediateUpdate = true;
                }
            }

            return immediateUpdate;
        }

        private bool CreateIntermediateUpdate(EditApprenticeshipCommand command, Party party, Apprenticeship apprenticeship)
        {
            if (command.EditApprenticeshipRequest.IntermediateApprenticeshipUpdateRequired())
            {
                var apprenticeshipUpdate = command.MapToApprenticeshipUpdate(apprenticeship, party, _currentDateTime.UtcNow);
                
                if (!string.IsNullOrWhiteSpace(apprenticeshipUpdate.TrainingCode))
                {
                    var result = _mediator.Send(new GetTrainingProgrammeQuery
                    {
                        Id = apprenticeshipUpdate.TrainingCode
                    }).Result;

                    apprenticeshipUpdate.TrainingName = result?.TrainingProgramme?.Name;
                    apprenticeshipUpdate.TrainingType = result?.TrainingProgramme?.ProgrammeType;
                }

                apprenticeship.CreateApprenticeshipUpdate(apprenticeshipUpdate, party);
                return true;
            }

            return false;
        }

        private async Task Validate(EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party, CancellationToken cancellationToken)
        {
            CheckPartyIsValid(party);

            var validationResult = await _editValidationService.Validate(command.CreateValidationRequest(apprenticeship, _currentDateTime.UtcNow ), cancellationToken);
            if (validationResult?.Errors?.Count > 0)
            {
                // This shouldn't get triggered as these checks should already been passed.
                // But in case someone is calling the EditApprenticeship endpoint directly
                string messages = string.Empty;
                validationResult.Errors.ForEach(x => messages += (x.PropertyName + ":" + x.ErrorMessage));
                _logger.LogError("Invalid operation for edit - the following error/s occured :" + messages);
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