﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate
{
    public class CreateApprenticeshipUpdateCommandHandler : AsyncRequestHandler<CreateApprenticeshipUpdateCommand>
    {
        private readonly AbstractValidator<CreateApprenticeshipUpdateCommand> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly IMediator _mediator;
        private readonly IHistoryRepository _historyRepository;
        private readonly ICommitmentRepository _commitmentRepository;
        private HistoryService _historyService;

        public CreateApprenticeshipUpdateCommandHandler(AbstractValidator<CreateApprenticeshipUpdateCommand> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, ICommitmentsLogger logger, IApprenticeshipRepository apprenticeshipRepository, IMediator mediator, IHistoryRepository historyRepository, ICommitmentRepository commitmentRepository)
        { 
            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _logger = logger;
            _apprenticeshipRepository = apprenticeshipRepository;
            _mediator = mediator;
            _historyRepository = historyRepository;
            _commitmentRepository = commitmentRepository;
        }

        protected override async Task HandleCore(CreateApprenticeshipUpdateCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            if (await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(command.ApprenticeshipUpdate.ApprenticeshipId) != null)
            {
                throw new ValidationException("Unable to create an ApprenticeshipUpdate for an Apprenticeship with a pending update");
            }

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipUpdate.ApprenticeshipId);

            if (!ValidateStartedApprenticeship(apprenticeship, command.ApprenticeshipUpdate))
                throw new ValidationException("Unable to create an update for an apprenticeship that is already started ");

            CheckAuthorisation(command, apprenticeship);
            await CheckOverlappingApprenticeships(command, apprenticeship);

            Apprenticeship immediateUpdate = null;
            if (HasImmediateUpdate(command))
            {
                await StartHistoryTracking(apprenticeship, command.Caller.CallerType, command.UserId);
                MapImmediateApprenticeshipUpdate(apprenticeship, command);
                immediateUpdate = apprenticeship;
            }
            
            var pendingUpdate = MapToPendingApprenticeshipUpdate(command.ApprenticeshipUpdate);

            await Task.WhenAll(
                _apprenticeshipUpdateRepository.CreateApprenticeshipUpdate(pendingUpdate, immediateUpdate),
                SaveHistory()
            );
        }

        private async Task SaveHistory()
        {
            if (_historyService != null)
            {
                await _historyService.Save();
            }
        }

        private async Task StartHistoryTracking(Apprenticeship apprenticeship, CallerType callerType, string userId)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);
            _historyService = new HistoryService(_historyRepository);
            _historyService.TrackUpdate(commitment, CommitmentChangeType.EditedApprenticeship.ToString(), commitment.Id, "Commitment", callerType, userId);
            _historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.Updated.ToString(), apprenticeship.Id, "Apprenticeship", callerType, userId);
        }

        private bool ValidateStartedApprenticeship(Apprenticeship apprenticeship, Api.Types.Apprenticeship.ApprenticeshipUpdate apprenticeshipUpdate)
        {
            var started = apprenticeship.StartDate.HasValue && apprenticeship.StartDate.Value <=
                                      new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (!started)
                return true;

            var isValid = 
                apprenticeshipUpdate.ULN == null
                && apprenticeshipUpdate.StartDate == null
                && apprenticeshipUpdate.EndDate == null
                && apprenticeshipUpdate.TrainingCode == null;

            if (!isValid)
            {
                _logger.Warn($"Trying to update a started apprenticeship with values; ULN {apprenticeshipUpdate.ULN}, StartDate: {apprenticeshipUpdate.StartDate}, EndDate: {apprenticeshipUpdate.EndDate}, TrainingCode: {apprenticeshipUpdate.TrainingCode}");
                return false;
            }

            return true;
        }

        private void MapImmediateApprenticeshipUpdate(Apprenticeship apprenticeship, CreateApprenticeshipUpdateCommand command)
        {
            apprenticeship.Id = command.ApprenticeshipUpdate.ApprenticeshipId;
            apprenticeship.ULN = command.ApprenticeshipUpdate.ULN;
            apprenticeship.ProviderRef = command.ApprenticeshipUpdate.ProviderRef;
            apprenticeship.EmployerRef = command.ApprenticeshipUpdate.EmployerRef;
        }

        private bool HasImmediateUpdate(CreateApprenticeshipUpdateCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.ApprenticeshipUpdate.ULN)
                && string.IsNullOrWhiteSpace(command.ApprenticeshipUpdate.EmployerRef)
                && string.IsNullOrWhiteSpace(command.ApprenticeshipUpdate.ProviderRef))
            {
                return false;
            }

            return true;
        }

        private ApprenticeshipUpdate MapToPendingApprenticeshipUpdate(Api.Types.Apprenticeship.ApprenticeshipUpdate update)
        {
            var result =  new ApprenticeshipUpdate
            {
                Id = update.Id,
                ApprenticeshipId = update.ApprenticeshipId,
                Originator = (Originator) update.Originator,
                FirstName = update.FirstName,
                LastName = update.LastName,
                DateOfBirth = update.DateOfBirth,
                TrainingCode = update.TrainingCode,
                TrainingType = update.TrainingType.HasValue ? (TrainingType) update.TrainingType : default(TrainingType?),
                TrainingName = update.TrainingName,
                Cost = update.Cost,
                StartDate = update.StartDate,
                EndDate = update.EndDate
            };

            return result.HasChanges ? result : null;
        }

        private void CheckAuthorisation(CreateApprenticeshipUpdateCommand command, Apprenticeship apprenticeship)
        {
            switch (command.Caller.CallerType)
            {
                case CallerType.Employer:
                    if(apprenticeship.EmployerAccountId != command.Caller.Id)
                        throw new UnauthorizedException($"Employer {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}");
                    break;
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != command.Caller.Id)
                        throw new UnauthorizedException($"Provider {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}");
                    break;
            }
        }

        private async Task CheckOverlappingApprenticeships(CreateApprenticeshipUpdateCommand command, Apprenticeship originalApprenticeship)
        {
            var coalesce = new Func<string, string, string>((s, s1) => string.IsNullOrWhiteSpace(s) ? s1 : s);

            var overlapResult = await _mediator.SendAsync(new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                    new ApprenticeshipOverlapValidationRequest
                    {
                        ApprenticeshipId = originalApprenticeship.Id,
                        Uln = coalesce(command.ApprenticeshipUpdate.ULN, originalApprenticeship.ULN),
                        StartDate = command.ApprenticeshipUpdate.StartDate ?? originalApprenticeship.StartDate.Value,
                        EndDate = command.ApprenticeshipUpdate.EndDate ?? originalApprenticeship.EndDate.Value
                    }
                }
            });

            if (overlapResult.Data.Any())
            {
                foreach (var overlap in overlapResult.Data)
                {
                    _logger.Info($"ApprenticeshipUpdate overlaps with apprenticeship {overlap.Apprenticeship.Id}");
                }
                throw new ValidationException("Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
            }
        }
    }
}
