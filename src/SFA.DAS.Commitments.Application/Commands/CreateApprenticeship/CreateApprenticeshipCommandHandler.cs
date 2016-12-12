using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipCommandHandler : IAsyncRequestHandler<CreateApprenticeshipCommand, long>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<CreateApprenticeshipCommand> _validator;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentsLogger _logger;

        public CreateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<CreateApprenticeshipCommand> validator, IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (validator == null)
                throw new ArgumentNullException(nameof(_validator));
            if (apprenticeshipEvents == null)
                throw new ArgumentNullException(nameof(_apprenticeshipEvents));

            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
        }

        public async Task<long> Handle(CreateApprenticeshipCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);

            CheckAuthorization(command, commitment);
            CheckEditStatus(command, commitment);
            CheckCommitmentStatus(command, commitment);

            var apprenticeshipId = await _commitmentRepository.CreateApprenticeship(MapFrom(command.Apprenticeship, command));

            command.Apprenticeship.Id = apprenticeshipId;

            await _apprenticeshipEvents.PublishEvent(commitment, MapFrom(command.Apprenticeship, command), "APPRENTICESHIP-CREATED");

            await UpdateStatusOfApprenticeship(commitment);

            return apprenticeshipId;
        }

        private static void CheckCommitmentStatus(CreateApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.CommitmentStatus != Domain.Entities.CommitmentStatus.New && commitment.CommitmentStatus != Domain.Entities.CommitmentStatus.Active)
                throw new InvalidOperationException($"Apprenticeship {message.Apprenticeship.Id} in commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(CreateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != Domain.Entities.EditStatus.Both && commitment.EditStatus != Domain.Entities.EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to edit apprenticeship {message.Apprenticeship.Id} in commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != Domain.Entities.EditStatus.Both && commitment.EditStatus != Domain.Entities.EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to edit apprenticeship {message.Apprenticeship.Id} in commitment {message.CommitmentId}");
                    break;
            }
        }

        private async Task UpdateStatusOfApprenticeship(Commitment commitment)
        {
            // TODO: Should we do just a blanket update accross all apprenticeships in the Commitment?
            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                if (apprenticeship.AgreementStatus != Domain.Entities.AgreementStatus.NotAgreed)
                {
                    await _commitmentRepository.UpdateApprenticeshipStatus(commitment.Id, apprenticeship.Id, Domain.Entities.AgreementStatus.NotAgreed);
                }
            }
        }

        private Domain.Entities.Apprenticeship MapFrom(Apprenticeship apprenticeship, CreateApprenticeshipCommand message)
        {
            var domainApprenticeship = new Domain.Entities.Apprenticeship
            {
                Id = apprenticeship.Id,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                DateOfBirth = apprenticeship.DateOfBirth,
                NINumber = apprenticeship.NINumber,
                ULN = apprenticeship.ULN,
                CommitmentId = message.CommitmentId,
                PaymentStatus = PaymentStatus.PendingApproval,
                AgreementStatus = (Domain.Entities.AgreementStatus) apprenticeship.AgreementStatus,
                TrainingType = (Domain.Entities.TrainingType) apprenticeship.TrainingType,
                TrainingCode = apprenticeship.TrainingCode,
                TrainingName = apprenticeship.TrainingName,
                Cost = apprenticeship.Cost,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate,
                EmployerRef = apprenticeship.EmployerRef,
                ProviderRef = apprenticeship.ProviderRef
            };

            return domainApprenticeship;
        }

        private static void CheckAuthorization(CreateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
            }
        }

        private void LogMessage(CreateApprenticeshipCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called CreateApprenticeshipCommand";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, commitmentId: command.CommitmentId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, commitmentId: command.CommitmentId);
        }

        private string BuildInfoMessage(CreateApprenticeshipCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called CreateApprenticeshipCommand";
        }
    }
}
