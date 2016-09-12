using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus
{
    public sealed class UpdateCommitmentStatusCommandHandler : AsyncRequestHandler<UpdateCommitmentStatusCommand>
    {
        private readonly AbstractValidator<UpdateCommitmentStatusCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;

        public UpdateCommitmentStatusCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<UpdateCommitmentStatusCommand> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        protected override async Task HandleCore(UpdateCommitmentStatusCommand message)
        {
            if (!_validator.Validate(message).IsValid)
            {
                throw new InvalidRequestException();
            }

            await _commitmentRepository.UpdateStatus(message.CommitmentId, (CommitmentStatus)message.Status);
        }
    }
}
