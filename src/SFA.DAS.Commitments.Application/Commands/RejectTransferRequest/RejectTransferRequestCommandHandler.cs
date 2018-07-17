using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.RejectTransferRequest
{
    public class RejectTransferRequestCommandHandler: AsyncRequestHandler<RejectTransferRequestCommand>
    {
        private readonly AbstractValidator<RejectTransferRequestCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _apprenticeshipEventsPublisher;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ICommitmentsLogger _logger;
        private readonly CohortApprovalService _cohortApprovalService;
        private readonly HistoryService _historyService;

        public RejectTransferRequestCommandHandler(AbstractValidator<RejectTransferRequestCommand> validator,
            ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipOverlapRules overlapRules, ICurrentDateTime currentDateTime,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, IMediator mediator,
            IMessagePublisher messagePublisher,
            IHistoryRepository historyRepository,
            ICommitmentsLogger logger)
        {
            _validator = validator;
            _commitmentRepository = commitmentRepository;
            _currentDateTime = currentDateTime;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _messagePublisher = messagePublisher;
            _logger = logger;
            _historyService = new HistoryService(historyRepository);

            _cohortApprovalService = new CohortApprovalService(apprenticeshipRepository, overlapRules, currentDateTime,
                commitmentRepository, apprenticeshipEventsList, apprenticeshipEventsPublisher, mediator, _logger);
        }

        protected override async Task HandleCore(RejectTransferRequestCommand command)
        {
            _validator.ValidateAndThrow(command);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            if (commitment == null)
            {
                throw new ResourceNotFoundException();
            }

            CheckAuthorization(command, commitment);
            CheckCommitmentStatus(commitment, command);

            _historyService.TrackUpdate(commitment, CommitmentChangeType.TransferSenderRejection.ToString(), commitment.Id, null, CallerType.TransferSender, command.UserEmail, commitment.ProviderId, command.TransferSenderId, command.UserName);

            await _commitmentRepository.SetTransferRequestApproval(command.TransferRequestId, command.CommitmentId,
                TransferApprovalStatus.TransferRejected, command.UserEmail, command.UserName);

            await _commitmentRepository.ResetEditStatusToEmployer(command.CommitmentId);

            await UpdateCommitmentObjectWithNewValues(commitment);

            await Task.WhenAll(
                 _cohortApprovalService.ResetApprenticeshipsAgreementStatuses(commitment),
                 _historyService.Save(),
                 PublishApprenticeshipUpdatedEvents(commitment),
                 PublishRejectedMessage(command)
            );
        }

        private async Task UpdateCommitmentObjectWithNewValues(Commitment commitment)
        {
            var updatedCommitment = await _commitmentRepository.GetCommitmentById(commitment.Id);
            commitment.TransferApprovalStatus = updatedCommitment.TransferApprovalStatus;
            commitment.TransferApprovalActionedByEmployerEmail = updatedCommitment.TransferApprovalActionedByEmployerEmail;
            commitment.TransferApprovalActionedByEmployerName = updatedCommitment.TransferApprovalActionedByEmployerName;
            commitment.TransferApprovalActionedOn = updatedCommitment.TransferApprovalActionedOn;
            commitment.EditStatus = updatedCommitment.EditStatus;
        }

    private async Task PublishApprenticeshipUpdatedEvents(Commitment commitment)
        {
            commitment.Apprenticeships.ForEach(apprenticeship =>
                _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED",
                    _currentDateTime.Now, null)
            );
            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);
        }

        private static void CheckAuthorization(RejectTransferRequestCommand message, Commitment commitment)
        {
            if (commitment.TransferSenderId != message.TransferSenderId)
                throw new UnauthorizedException(
                    $"Employer {message.TransferSenderId} not authorised to access commitment: {message.CommitmentId} as transfer sender, expected transfer sender {commitment.TransferSenderId}");
        }

        private static void CheckCommitmentStatus(Commitment commitment, RejectTransferRequestCommand command)
        {
            if (commitment.EmployerAccountId != command.TransferReceiverId)
                throw new InvalidOperationException($"Commitment {commitment.Id} has employer account Id {commitment.EmployerAccountId} which doesn't match command receiver Id {command.TransferReceiverId}");

            if (commitment.CommitmentStatus == CommitmentStatus.Deleted)
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");

            if (commitment.TransferApprovalStatus != TransferApprovalStatus.Pending)
                throw new InvalidOperationException($"Transfer Approval for Commitment {commitment.Id} cannot be set because the status is {commitment.TransferApprovalStatus}");

            if (commitment.EditStatus != EditStatus.Both)
                throw new InvalidOperationException($"Transfer Sender {commitment.TransferSenderId} not allowed to reject until both the provider and receiving employer have approved");
        }

        private async Task PublishRejectedMessage(RejectTransferRequestCommand command)
        {
            var message = new CohortRejectedByTransferSender(command.TransferRequestId, command.TransferReceiverId, command.CommitmentId,
                command.TransferSenderId, command.UserName, command.UserEmail);
            await _messagePublisher.PublishAsync(message);         
        }
    }
}
