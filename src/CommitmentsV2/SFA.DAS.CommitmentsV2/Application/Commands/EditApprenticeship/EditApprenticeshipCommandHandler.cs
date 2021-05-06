using MediatR;
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
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship
{
    public class EditApprenticeshipCommandHandler : IRequestHandler<EditApprenticeshipCommand, EditApprenticeshipResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IEditApprenticeshipValidationService _editValidationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly ILogger<EditApprenticeshipCommandHandler> _logger;
        private readonly IMediator _mediator;

        public EditApprenticeshipCommandHandler(IEditApprenticeshipValidationService editValidationService, Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService, ICurrentDateTime currentDateTime,  IMediator mediator, ILogger<EditApprenticeshipCommandHandler> logger)
        {
            _editValidationService = editValidationService;
            _dbContext = dbContext;
            _authenticationService = authenticationService;
            _currentDateTime = currentDateTime;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<EditApprenticeshipResponse> Handle(EditApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            var party = _authenticationService.GetUserParty();
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.EditApprenticeshipRequest.ApprenticeshipId, cancellationToken);

            await Validate(command, apprenticeship, party, cancellationToken);

            CreateImmedidateUpdate(command, party, apprenticeship);

            var immediateUpdateCreated = CreateIntermediateUpdate(command, party, apprenticeship);

            return new EditApprenticeshipResponse { ApprenticeshipId = command.EditApprenticeshipRequest.ApprenticeshipId, NeedReapproval = immediateUpdateCreated };
        }

        private void CreateImmedidateUpdate(EditApprenticeshipCommand command, Party party, Apprenticeship apprenticeship)
        {
            if (command.EmployerReferenceUpdateRequired(apprenticeship, party))
            {
                apprenticeship.UpdateEmployerReference(command.EditApprenticeshipRequest.EmployerReference, party, command.EditApprenticeshipRequest.UserInfo);
            }
            else
            {
                if (command.ProviderReferenceUpdateRequired(apprenticeship, party))
                {
                    apprenticeship.UpdateProviderReference(command.EditApprenticeshipRequest.ProviderReference, party, command.EditApprenticeshipRequest.UserInfo);
                }

                if (command.ULNUpdateRequired(apprenticeship, party))
                {
                    apprenticeship.UpdateULN(command.EditApprenticeshipRequest.ULN, party, _currentDateTime.UtcNow, command.EditApprenticeshipRequest.UserInfo);
                }
            }
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
            CheckAuthorisation(command, apprenticeship, party);

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

            if (apprenticeship.ApprenticeshipUpdate.Any(a => a.Status == ApprenticeshipUpdateStatus.Pending))
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

        private void CheckAuthorisation(EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party)
        {
            switch (party)
            {
                case Party.Employer:
                    if (apprenticeship.Cohort.EmployerAccountId != command.EditApprenticeshipRequest.AccountId)
                        throw new UnauthorizedAccessException($"Employer {command.EditApprenticeshipRequest.UserInfo.UserId} not authorised to update apprenticeship {apprenticeship.Id}");
                    break;
            }
        }
    }
}
