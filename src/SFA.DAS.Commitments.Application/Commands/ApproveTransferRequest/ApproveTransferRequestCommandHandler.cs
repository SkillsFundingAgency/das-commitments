using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Messaging.Interfaces;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using TransferApprovalStatus = SFA.DAS.Commitments.Domain.Entities.TransferApprovalStatus;

namespace SFA.DAS.Commitments.Application.Commands.ApproveTransferRequest
{
    public sealed class ApproveTransferRequestCommandHandler : AsyncRequestHandler<ApproveTransferRequestCommand>
    {
        private readonly AbstractValidator<ApproveTransferRequestCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IV2EventsPublisher _v2EventsPublisher;

        public ApproveTransferRequestCommandHandler(AbstractValidator<ApproveTransferRequestCommand> validator,
            ICommitmentRepository commitmentRepository, 
            IV2EventsPublisher v2EventsPublisher = null)
        {
            _validator = validator;
            _commitmentRepository = commitmentRepository;
            _v2EventsPublisher = v2EventsPublisher;
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

            await _v2EventsPublisher.SendApproveTransferRequestCommand(command.TransferRequestId, DateTime.UtcNow,
                new UserInfo {UserEmail = command.UserEmail, UserDisplayName = command.UserName});
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
    }
}