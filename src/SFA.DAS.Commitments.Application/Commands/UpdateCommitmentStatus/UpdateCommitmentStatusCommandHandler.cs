using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus
{
    public sealed class UpdateCommitmentStatusCommandHandler : AsyncRequestHandler<UpdateCommitmentStatusCommand>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly AbstractValidator<UpdateCommitmentStatusCommand> _validator;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentRepository _commitmentRepository;

        public UpdateCommitmentStatusCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<UpdateCommitmentStatusCommand> validator, IApprenticeshipEvents apprenticeshipEvents)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _apprenticeshipEvents = apprenticeshipEvents;
        }

        protected override async Task HandleCore(UpdateCommitmentStatusCommand message)
        {
            Logger.Info($"{message.Caller.CallerType}: {message.Caller.Id} has called UpdateCommitmentStatusCommand");

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckAuthorization(message, commitment);

            if (commitment.CommitmentStatus != (CommitmentStatus) message.CommitmentStatus)
            {
                await _commitmentRepository.UpdateCommitmentStatus(message.CommitmentId, (CommitmentStatus) message.CommitmentStatus);

                foreach (var apprenticeship in commitment.Apprenticeships)
                {
                    await _apprenticeshipEvents.PublishEvent(commitment, apprenticeship, "COMMITMENT-STATUS-UPDATED");
                }
            }
        }

        private static void CheckAuthorization(UpdateCommitmentStatusCommand message, Commitment commitment)
        {
            if (message.Caller.CallerType == CallerType.Provider)
            {
                if (commitment.ProviderId != message.Caller.Id)
                    throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view commitment {message.CommitmentId}");
            }
            else
            {
                if (commitment.EmployerAccountId != message.Caller.Id)
                    throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view commitment {message.CommitmentId}");
            }
        }
    }
}
