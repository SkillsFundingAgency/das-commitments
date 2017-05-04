using System;
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

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate
{
    public class UpdateApprenticeshipUpdateCommandHandler : AsyncRequestHandler<UpdateApprenticeshipUpdateCommand>
    {
        private readonly AbstractValidator<UpdateApprenticeshipUpdateCommand> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IMediator _mediator;
        private readonly IUpdateApprenticeshipUpdateMapper _mapper;
        private readonly IApprenticeshipEvents _eventsApi;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHistoryRepository _historyRepository;

        public UpdateApprenticeshipUpdateCommandHandler(AbstractValidator<UpdateApprenticeshipUpdateCommand> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, IApprenticeshipRepository apprenticeshipRepository, IMediator mediator, IUpdateApprenticeshipUpdateMapper mapper, IApprenticeshipEvents eventsApi, ICommitmentRepository commitmentRepository, IHistoryRepository historyRepository)
        {
            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _mediator = mediator;
            _mapper = mapper;
            _eventsApi = eventsApi;
            _commitmentRepository = commitmentRepository;
            _historyRepository = historyRepository;
        }

        protected override async Task HandleCore(UpdateApprenticeshipUpdateCommand command)
        {
            var pendingUpdate = await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(command.ApprenticeshipId);
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            await ValidateCommand(command, pendingUpdate, apprenticeship);

            if (command.UpdateStatus == ApprenticeshipUpdateStatus.Approved)
            {
                await ApproveApprenticeshipUpdate(command, apprenticeship, pendingUpdate);
            }

            if (command.UpdateStatus == ApprenticeshipUpdateStatus.Rejected)
            {
                await _apprenticeshipUpdateRepository.RejectApprenticeshipUpdate(pendingUpdate.Id, command.UserId);
            }

            if (command.UpdateStatus == ApprenticeshipUpdateStatus.Deleted)
            {
                await _apprenticeshipUpdateRepository.UndoApprenticeshipUpdate(pendingUpdate.Id, command.UserId);
            }
        }

        private async Task ApproveApprenticeshipUpdate(UpdateApprenticeshipUpdateCommand command, Apprenticeship apprenticeship, ApprenticeshipUpdate pendingUpdate)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(commitment, CommitmentChangeType.EditedApprenticeship.ToString(), commitment.Id, "Commitment", command.Caller.CallerType, command.UserId, command.UserName);
            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.Updated.ToString(), apprenticeship.Id, "Apprenticeship", command.Caller.CallerType, command.UserId, command.UserName);
            var originalApprenticeship = apprenticeship.Clone();
            _mapper.ApplyUpdate(apprenticeship, pendingUpdate);

            await Task.WhenAll(
                _apprenticeshipUpdateRepository.ApproveApprenticeshipUpdate(pendingUpdate.Id, command.UserId, apprenticeship, command.Caller),
                CreateEvents(commitment, originalApprenticeship, apprenticeship, pendingUpdate),
                historyService.Save()
            );
        }

        private async Task CreateEvents(Commitment commitment, Apprenticeship apprenticeship, Apprenticeship updatedApprenticeship, ApprenticeshipUpdate pendingUpdate)
        {
            DateTime? changeEffective = pendingUpdate.CreatedOn;
            if (updatedApprenticeship.StartDate > pendingUpdate.CreatedOn) // Was waiting to start when created
            {
                changeEffective = updatedApprenticeship.StartDate;
            }

            await _eventsApi.PublishEvent(commitment, apprenticeship, "APPRENTICESHIP-UPDATED", null, changeEffective?.AddDays(-1));
            await _eventsApi.PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-UPDATED", changeEffective, null);
        }

        private async Task ValidateCommand(UpdateApprenticeshipUpdateCommand command, ApprenticeshipUpdate pendingUpdate, Apprenticeship apprenticeship)
        {
            var result = _validator.Validate(command);
            if (!result.IsValid)
                throw new ValidationException("Did not validate");

            if (pendingUpdate == null)
                throw new ValidationException($"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");
            
            CheckAuthorisation(command, apprenticeship);

            if (command.UpdateStatus == ApprenticeshipUpdateStatus.Approved)
            {
                await CheckOverlappingApprenticeships(pendingUpdate, apprenticeship);
            }
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

        private void CheckAuthorisation(UpdateApprenticeshipUpdateCommand command, Apprenticeship apprenticeship)
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
    }
}