using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

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
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _apprenticeshipEventsPublisher;
        private readonly IReservationValidationService _reservationValidationService;
        private readonly IV2EventsPublisher _v2EventsPublisher;

        public CreateApprenticeshipUpdateCommandHandler(AbstractValidator<CreateApprenticeshipUpdateCommand> validator,
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, ICommitmentsLogger logger,
            IApprenticeshipRepository apprenticeshipRepository, IMediator mediator,
            IHistoryRepository historyRepository, ICommitmentRepository commitmentRepository,
            ICurrentDateTime currentDateTime, IMessagePublisher messagePublisher,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher,
            IReservationValidationService reservationValidationService,
            IV2EventsPublisher v2EventsPublisher)
        {
            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _logger = logger;
            _apprenticeshipRepository = apprenticeshipRepository;
            _mediator = mediator;
            _historyRepository = historyRepository;
            _commitmentRepository = commitmentRepository;
            _currentDateTime = currentDateTime;
            _messagePublisher = messagePublisher;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _reservationValidationService = reservationValidationService;
            _v2EventsPublisher = v2EventsPublisher;
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

            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            if (!ValidateStartedApprenticeship(apprenticeship, command.ApprenticeshipUpdate))
                throw new ValidationException("Unable to create an update for an apprenticeship that is already started ");

            CheckAuthorisation(command, apprenticeship);

            await Task.WhenAll(
                CheckOverlappingApprenticeships(command, apprenticeship),
                CheckReservation(command.ApprenticeshipUpdate, apprenticeship));

            Apprenticeship immediateUpdate = null;
            ApprenticeshipUpdate pendingUpdate = null;

            if (HasImmediateUpdate(command))
            {
                StartHistoryTracking(commitment, apprenticeship, command.Caller.CallerType, command.UserId, command.UserName);
                MapImmediateApprenticeshipUpdate(apprenticeship, command);
                immediateUpdate = apprenticeship;
            }

            if (apprenticeship.StartDate == null)
            {
                throw new InvalidOperationException($"The start date on apprenticeship {apprenticeship.Id} is null when calling {nameof(CreateApprenticeshipUpdateCommand)} command handler");
            }

            if (command.ApprenticeshipUpdate.HasChanges)
            {
                pendingUpdate = command.ApprenticeshipUpdate;
                pendingUpdate.EffectiveFromDate = apprenticeship.StartDate.Value;
                await SendApprenticeshipUpdateCreatedEvent(apprenticeship);
            }

            var tasksToRun = new List<Task>
            {
                _apprenticeshipUpdateRepository.CreateApprenticeshipUpdate(pendingUpdate, immediateUpdate),
                SaveHistory()
            };

            if (!string.IsNullOrEmpty(command.ApprenticeshipUpdate.ULN))
            {
                tasksToRun.Add(_v2EventsPublisher.PublishApprenticeshipUlnUpdatedEvent(immediateUpdate));

                _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-UPDATED", _currentDateTime.Now);
                tasksToRun.Add(_apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList));
            }

            await Task.WhenAll(tasksToRun);
        }

        private async Task SendApprenticeshipUpdateCreatedEvent(Apprenticeship apprenticeship)
        {
            await _messagePublisher.PublishAsync(new ApprenticeshipUpdateCreated(apprenticeship.EmployerAccountId, apprenticeship.ProviderId, apprenticeship.Id));
        }

        private async Task SaveHistory()
        {
            if (_historyService != null)
            {
                await _historyService.Save();
            }
        }

        private void StartHistoryTracking(Commitment commitment, Apprenticeship apprenticeship, CallerType callerType, string userId, string userName)
        {
            _historyService = new HistoryService(_historyRepository);
            _historyService.TrackUpdate(commitment, CommitmentChangeType.EditedApprenticeship.ToString(), commitment.Id, null, callerType, userId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, userName);
            _historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.Updated.ToString(), null, apprenticeship.Id, callerType, userId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, userName);
        }

        private bool ValidateStartedApprenticeship(Apprenticeship apprenticeship, ApprenticeshipUpdate apprenticeshipUpdate)
        {
            var started = apprenticeship.StartDate.HasValue && apprenticeship.StartDate.Value <=
                                      new DateTime(_currentDateTime.Now.Year, _currentDateTime.Now.Month, 1);

            if (!started)
                return true;

            if (apprenticeship.HasHadDataLockSuccess &&
                (apprenticeshipUpdate.Cost != null || apprenticeshipUpdate.TrainingCode != null)
                )
            {
                _logger.Warn($"Trying to update a started apprenticeship with a successful DataLock with values; Cost {apprenticeshipUpdate.Cost}, TrainingCode: {apprenticeshipUpdate.TrainingCode}");
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
                && command.ApprenticeshipUpdate.EmployerRef == null
                && command.ApprenticeshipUpdate.ProviderRef == null)
            {
                return false;
            }

            return true;
        }

        private void CheckAuthorisation(CreateApprenticeshipUpdateCommand command, Apprenticeship apprenticeship)
        {
            switch (command.Caller.CallerType)
            {
                case CallerType.Employer:
                    if (apprenticeship.EmployerAccountId != command.Caller.Id)
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
            if (originalApprenticeship.StartDate == null)
            {
                throw new InvalidOperationException($"The start date on apprenticeship {originalApprenticeship.Id} is null when calling {nameof(CheckOverlappingApprenticeships)}");
            }

            if (originalApprenticeship.EndDate == null)
            {
                throw new InvalidOperationException($"The end date on apprenticeship {originalApprenticeship.Id} is null when calling {nameof(CheckOverlappingApprenticeships)}");
            }

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
                    _logger.Info($"ApprenticeshipUpdate overlaps with apprenticeship {overlap.Id}");
                }
                throw new ValidationException("Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
            }
        }

        private async Task CheckReservation(ApprenticeshipUpdate pendingUpdate, Apprenticeship originalApprenticeship)
        {
            var request = new ReservationValidationServiceRequest
            {
                AccountId = originalApprenticeship.EmployerAccountId,
                ApprenticeshipId = originalApprenticeship.Id,
                ReservationId = originalApprenticeship.ReservationId,
                CommitmentId = originalApprenticeship.CommitmentId,
                TrainingCode = pendingUpdate.TrainingCode ?? originalApprenticeship.TrainingCode,
                StartDate = pendingUpdate.StartDate ?? originalApprenticeship.StartDate,
                ProviderId = originalApprenticeship.ProviderId
            };

            var validationResult = await _reservationValidationService.CheckReservation(request);

            if (validationResult.HasErrors)
            {
                var validationFailures =
                    validationResult.ValidationErrors.Select(e => new ValidationFailure(e.PropertyName, e.Reason));

                throw new ValidationException(validationFailures);
            }
        }
    }
}
