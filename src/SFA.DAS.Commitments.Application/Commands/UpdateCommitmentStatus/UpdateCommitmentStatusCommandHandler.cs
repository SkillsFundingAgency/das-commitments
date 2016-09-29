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
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        protected override async Task HandleCore(UpdateCommitmentStatusCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckAuthorization(message, commitment);

            if (message.Status.HasValue && commitment.Status != (CommitmentStatus) message.Status.Value)
            {
                await _commitmentRepository.UpdateStatus(message.CommitmentId, (CommitmentStatus)message.Status);
            }
        }

        private static void CheckAuthorization(UpdateCommitmentStatusCommand message, Domain.Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer unauthorized to view commitment: {message.CommitmentId}");
        }
    }
}
