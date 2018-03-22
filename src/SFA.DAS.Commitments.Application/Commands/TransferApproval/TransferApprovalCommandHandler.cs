using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.EmployerApproveCohort;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
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

namespace SFA.DAS.Commitments.Application.Commands.TransferApproval
{
    public sealed class TransferApprovalCommandHandler : AsyncRequestHandler<TransferApprovalCommand>
    {
        private readonly AbstractValidator<TransferApprovalCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly CohortApprovalService _cohortApprovalService;
        private readonly HistoryService _historyService;

        public TransferApprovalCommandHandler(AbstractValidator<TransferApprovalCommand> validator,
            ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipOverlapRules overlapRules, ICurrentDateTime currentDateTime,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, IMediator mediator,
            IMessagePublisher messagePublisher,
            IHistoryRepository historyRepository)
        {
            _validator = validator;
            _commitmentRepository = commitmentRepository;
            _messagePublisher = messagePublisher;
            _historyService = new HistoryService(historyRepository);

            _cohortApprovalService = new CohortApprovalService(apprenticeshipRepository, overlapRules, currentDateTime,
                commitmentRepository, apprenticeshipEventsList, apprenticeshipEventsPublisher, mediator);

        }

        protected override async Task HandleCore(TransferApprovalCommand command)
        {
            _validator.ValidateAndThrow(command);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            if (commitment == null)
            {
                throw new ResourceNotFoundException();
            }

            CheckAuthorization(command, commitment);
            CheckCommitmentStatus(commitment, command);

            await _commitmentRepository.SetTransferApproval(command.CommitmentId, command.TransferApprovalStatus,
                command.UserEmail, command.UserName);

            if (command.TransferApprovalStatus == TransferApprovalStatus.TransferApproved)
            {
                commitment.TransferApprovalStatus = TransferApprovalStatus.TransferApproved;
                await Task.WhenAll(
                    _cohortApprovalService.UpdateApprenticeshipsPaymentStatusToPaid(commitment),
                    _cohortApprovalService.CreatePriceHistory(commitment),
                    _cohortApprovalService.PublishApprenticeshipEventsWhenTransferSenderHasApproved(commitment),
                    _cohortApprovalService.ReorderPayments(commitment.EmployerAccountId));
                _historyService.TrackUpdate(commitment, CommitmentChangeType.TransferSenderApproval.ToString(), commitment.Id, null, CallerType.TransferSender, command.UserEmail, commitment.ProviderId, command.TransferSenderId, command.UserName);
                await _historyService.Save();
            }

            await PublishApprovedOrRejectedMessage(command);

        }

        private static void CheckAuthorization(TransferApprovalCommand message, Commitment commitment)
        {
            if (commitment.TransferSenderId != message.TransferSenderId)
                throw new UnauthorizedException(
                    $"Employer {message.TransferSenderId} not authorised to access commitment: {message.CommitmentId} as transfer sender, expected transfer sender {commitment.TransferSenderId}");
        }

        private static void CheckCommitmentStatus(Commitment commitment, TransferApprovalCommand command)
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

        private async Task PublishApprovedOrRejectedMessage(TransferApprovalCommand command)
        {
            switch (command.TransferApprovalStatus)
            {
                case TransferApprovalStatus.TransferApproved:
                {
                    var message = new CohortApprovedByTransferSender(command.TransferReceiverId, command.CommitmentId,
                        command.TransferSenderId, command.UserName, command.UserEmail);
                    await _messagePublisher.PublishAsync(message);
                    break;
                }
                case TransferApprovalStatus.TransferRejected:
                {
                    var message = new CohortRejectedByTransferSender(command.TransferReceiverId, command.CommitmentId,
                        command.TransferSenderId, command.UserName, command.UserEmail);
                    await _messagePublisher.PublishAsync(message);
                    break;
                }
                default:
                    throw new Exception("The Transfer Approval Status is not Approved or Rejected");
            }
        }



    }
}

