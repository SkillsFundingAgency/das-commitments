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

namespace SFA.DAS.Commitments.Application.Commands.ApproveTransferRequest
{
    public sealed class ApproveTransferRequestCommandHandler : AsyncRequestHandler<ApproveTransferRequestCommand>
    {
        private readonly AbstractValidator<ApproveTransferRequestCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ICommitmentsLogger _logger;
        private readonly CohortApprovalService _cohortApprovalService;
        private readonly HistoryService _historyService;

        public ApproveTransferRequestCommandHandler(AbstractValidator<ApproveTransferRequestCommand> validator,
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
            _messagePublisher = messagePublisher;
            _logger = logger;
            _historyService = new HistoryService(historyRepository);

            _cohortApprovalService = new CohortApprovalService(apprenticeshipRepository, overlapRules, currentDateTime,
                commitmentRepository, apprenticeshipEventsList, apprenticeshipEventsPublisher, mediator, _logger);

        }

        protected override async Task HandleCore(ApproveTransferRequestCommand command)
        {
            _validator.ValidateAndThrow(command);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            if (commitment == null)
            {
                throw new ResourceNotFoundException();
            }

            CheckAuthorization(command, commitment);
            CheckCommitmentStatus(commitment, command);

            _historyService.TrackUpdate(commitment, CommitmentChangeType.TransferSenderApproval.ToString(), commitment.Id, null, CallerType.TransferSender, command.UserEmail, commitment.ProviderId, command.TransferSenderId, command.UserName);

            await _commitmentRepository.SetTransferRequestApproval(command.TransferRequestId, command.CommitmentId,
                TransferApprovalStatus.TransferApproved, command.UserEmail, command.UserName);

            await UpdateCommitmentObjectWithNewValues(commitment);

            await Task.WhenAll(
                _cohortApprovalService.UpdateApprenticeshipsPaymentStatusToPaid(commitment),
                _cohortApprovalService.CreatePriceHistory(commitment),
                _cohortApprovalService.PublishApprenticeshipEventsWhenTransferSenderHasApproved(commitment),
                _cohortApprovalService.ReorderPayments(commitment.EmployerAccountId),
                _historyService.Save());

            await PublishApprovedMessage(command);
        }

        private async Task UpdateCommitmentObjectWithNewValues(Commitment commitment)
        {
            var updatedCommitment = await _commitmentRepository.GetCommitmentById(commitment.Id);
            commitment.TransferApprovalStatus = updatedCommitment.TransferApprovalStatus;
            commitment.TransferApprovalActionedByEmployerEmail = updatedCommitment.TransferApprovalActionedByEmployerEmail;
            commitment.TransferApprovalActionedByEmployerName = updatedCommitment.TransferApprovalActionedByEmployerName;
            commitment.TransferApprovalActionedOn = updatedCommitment.TransferApprovalActionedOn;
        }

    private static void CheckAuthorization(ApproveTransferRequestCommand message, Commitment commitment)
        {
            if (commitment.TransferSenderId != message.TransferSenderId)
                throw new UnauthorizedException(
                    $"Employer {message.TransferSenderId} not authorised to access commitment: {message.CommitmentId} as transfer sender, expected transfer sender {commitment.TransferSenderId}");
        }

        private static void CheckCommitmentStatus(Commitment commitment, ApproveTransferRequestCommand command)
        {
            if (commitment.EmployerAccountId != command.TransferReceiverId)
                throw new InvalidOperationException($"Commitment {commitment.Id} has employer account Id {commitment.EmployerAccountId} which doesn't match command receiver Id {command.TransferReceiverId}");

            if (commitment.CommitmentStatus == CommitmentStatus.Deleted)
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");

            if (commitment.TransferApprovalStatus != TransferApprovalStatus.Pending)
                throw new InvalidOperationException($"Transfer Approval for Commitment {commitment.Id} cannot be set because the status is {commitment.TransferApprovalStatus}");
            
            if (commitment.EditStatus != EditStatus.Both)
                throw new InvalidOperationException($"Transfer Sender {commitment.TransferSenderId} not allowed to approve until both the provider and receiving employer have approved");
        }

        private async Task PublishApprovedMessage(ApproveTransferRequestCommand command)
        {
            var message = new CohortApprovedByTransferSender(command.TransferRequestId, command.TransferReceiverId, command.CommitmentId,
                command.TransferSenderId, command.UserName, command.UserEmail);
            await _messagePublisher.PublishAsync(message);
        }



    }
}

