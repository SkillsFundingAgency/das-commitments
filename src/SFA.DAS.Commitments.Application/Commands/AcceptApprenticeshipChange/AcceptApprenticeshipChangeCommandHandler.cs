using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange
{
    public class AcceptApprenticeshipChangeCommandHandler : AsyncRequestHandler<AcceptApprenticeshipChangeCommand>
    {
        private readonly AbstractValidator<AcceptApprenticeshipChangeCommand> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IMediator _mediator;
        private readonly IAcceptApprenticeshipChangeMapper _mapper;
        private readonly IApprenticeshipEvents _eventsApi;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IV2EventsPublisher _v2EventsPublisher;

        public AcceptApprenticeshipChangeCommandHandler(
            AbstractValidator<AcceptApprenticeshipChangeCommand> validator,
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IMediator mediator,
            IAcceptApprenticeshipChangeMapper mapper,
            IApprenticeshipEvents eventsApi,
            ICommitmentRepository commitmentRepository,
            IHistoryRepository historyRepository,
            IMessagePublisher messagePublisher,
            IV2EventsPublisher v2EventsPublisher)
        {
            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _mediator = mediator;
            _mapper = mapper;
            _eventsApi = eventsApi;
            _commitmentRepository = commitmentRepository;
            _historyRepository = historyRepository;
            _messagePublisher = messagePublisher;
            _v2EventsPublisher = v2EventsPublisher;
        }

        protected override async Task HandleCore(AcceptApprenticeshipChangeCommand command)
        {
            var pendingUpdate = await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(command.ApprenticeshipId);
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            await ValidateCommand(command, pendingUpdate, apprenticeship);

            await ApproveApprenticeshipUpdate(command, apprenticeship, pendingUpdate);

        }

        private async Task ApproveApprenticeshipUpdate(AcceptApprenticeshipChangeCommand command, Apprenticeship apprenticeship, ApprenticeshipUpdate pendingUpdate)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);
            
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(commitment, CommitmentChangeType.EditedApprenticeship.ToString(), commitment.Id, null, command.Caller.CallerType, command.UserId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, command.UserName);
            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.Updated.ToString(), null, apprenticeship.Id, command.Caller.CallerType, command.UserId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, command.UserName);
            var originalApprenticeship = apprenticeship.Clone();
            _mapper.ApplyUpdate(apprenticeship, pendingUpdate);

            await Task.WhenAll(
                _apprenticeshipUpdateRepository.ApproveApprenticeshipUpdate(pendingUpdate, apprenticeship, command.Caller),
                CreateEvents(commitment, originalApprenticeship, apprenticeship, pendingUpdate),
                historyService.Save(),
                _messagePublisher.PublishAsync(new ApprenticeshipUpdateAccepted(commitment.EmployerAccountId, commitment.ProviderId.Value, command.ApprenticeshipId)),
                _v2EventsPublisher.PublishApprenticeshipUpdatedApproved(commitment, apprenticeship)
            );
        }

        private async Task CreateEvents(Commitment commitment, Apprenticeship apprenticeship, Apprenticeship updatedApprenticeship, ApprenticeshipUpdate pendingUpdate)
        {
            await _eventsApi.PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-UPDATED", pendingUpdate.EffectiveFromDate, null);
        }

        private async Task ValidateCommand(AcceptApprenticeshipChangeCommand command, ApprenticeshipUpdate pendingUpdate, Apprenticeship apprenticeship)
        {
            var result = _validator.Validate(command);
            if (!result.IsValid)
                throw new ValidationException("Did not validate");

            if (pendingUpdate == null)
                throw new ValidationException($"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");

            CheckAuthorisation(command, apprenticeship);

            await CheckOverlappingApprenticeships(pendingUpdate, apprenticeship);

        }

        private async Task CheckOverlappingApprenticeships(ApprenticeshipUpdate pendingUpdate, Apprenticeship originalApprenticeship)
        {
            var overlapResult = await _mediator.SendAsync(new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                    new ApprenticeshipOverlapValidationRequest
                    {
                        ApprenticeshipId = originalApprenticeship.Id,
                        Uln = originalApprenticeship.ULN,
                        StartDate = pendingUpdate.StartDate ?? originalApprenticeship.StartDate.Value,
                        EndDate = pendingUpdate.EndDate ?? originalApprenticeship.EndDate.Value
                    }
                }
            });

            if (overlapResult.Data.Any())
            {
                throw new ValidationException("Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
            }
        }


        private void CheckAuthorisation(AcceptApprenticeshipChangeCommand command, Apprenticeship apprenticeship)
        {
            switch (command.Caller.CallerType)
            {
                case CallerType.Employer:
                    if (apprenticeship.EmployerAccountId != command.Caller.Id)
                        throw new UnauthorizedException($"Employer {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}");
                    break;
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != command.Caller.Id)
                        throw new UnauthorizedException($"Provider {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}, expected provider {apprenticeship.ProviderId}");
                    break;
            }
        }
    }
}
