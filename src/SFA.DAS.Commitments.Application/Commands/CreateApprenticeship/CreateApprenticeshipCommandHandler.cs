using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipCommandHandler : IAsyncRequestHandler<CreateApprenticeshipCommand, long>
    {
        private readonly ICommitmentRepository _commitmentRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly AbstractValidator<CreateApprenticeshipCommand> _validator;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private IMessagePublisher _messagePublisher;
        private readonly ICohortTransferService _cohortTransferService;

        public CreateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<CreateApprenticeshipCommand> validator, IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger, IHistoryRepository historyRepository, IMessagePublisher messagePublisher, ICohortTransferService cohortTransferService)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
            _historyRepository = historyRepository;
            _messagePublisher = messagePublisher;
            _cohortTransferService = cohortTransferService;
        }

        public async Task<long> Handle(CreateApprenticeshipCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // TODO: Throw Exception if commitment doesn't exist
            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);

            CheckAuthorization(command, commitment);
            CheckEditStatus(command, commitment);
            CheckCommitmentStatus(commitment);

            var publishMessage = command.Caller.CallerType == CallerType.Employer
                && commitment.Apprenticeships.Any()
                && commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.ProviderAgreed);

            var apprenticeship = UpdateApprenticeship(command.Apprenticeship, command);
            var apprenticeshipId = await _apprenticeshipRepository.CreateApprenticeship(apprenticeship);
            var savedApprenticeship = await _apprenticeshipRepository.GetApprenticeship(apprenticeshipId);

            await Task.WhenAll(
                _apprenticeshipEvents.PublishEvent(commitment, savedApprenticeship, "APPRENTICESHIP-CREATED"),
                UpdateStatusOfApprenticeship(commitment),
                CreateHistory(commitment, savedApprenticeship, command.Caller.CallerType, command.UserId, command.UserName),
                 _cohortTransferService.ResetCommitmentTransferRejection(commitment, command.UserId, command.UserName)
            );

            if (publishMessage)
            {
                await PublishMessage(commitment);
            }

            return apprenticeshipId;
        }

        private async Task PublishMessage(Commitment commitment)
        {
            await _messagePublisher.PublishAsync(
                      new ProviderCohortApprovalUndoneByEmployerUpdate(
                          commitment.EmployerAccountId,
                          commitment.ProviderId.Value,
                          commitment.Id));
        }

        private Apprenticeship UpdateApprenticeship(Apprenticeship apprenticeship, CreateApprenticeshipCommand command)
        {
            apprenticeship.CommitmentId = command.CommitmentId;
            apprenticeship.PaymentStatus = PaymentStatus.PendingApproval;
            return apprenticeship;
        }

        private async Task CreateHistory(Commitment commitment, Domain.Entities.Apprenticeship apprenticeship, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(commitment, CommitmentChangeType.CreatedApprenticeship.ToString(), commitment.Id, null, callerType, userId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, userName);
            historyService.TrackInsert(apprenticeship, ApprenticeshipChangeType.Created.ToString(), null, apprenticeship.Id, callerType, userId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, userName);
            await historyService.Save();
        }

        private async Task UpdateStatusOfApprenticeship(Commitment commitment)
        {
            // TODO: Should we do just a blanket update accross all apprenticeships in the Commitment?
            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                if (apprenticeship.AgreementStatus != Domain.Entities.AgreementStatus.NotAgreed)
                {
                    await _apprenticeshipRepository.UpdateApprenticeshipStatus(commitment.Id, apprenticeship.Id, Domain.Entities.AgreementStatus.NotAgreed);
                }
            }
        }

        private static void CheckAuthorization(CreateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not authorised to access commitment: {message.CommitmentId}, expected provider {commitment.ProviderId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not authorised to access commitment: {message.CommitmentId}, expected employer {commitment.EmployerAccountId}");
                    break;
            }
        }

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus != Domain.Entities.CommitmentStatus.New && commitment.CommitmentStatus != Domain.Entities.CommitmentStatus.Active)
                throw new InvalidOperationException($"Cannot add apprenticeship in commitment {commitment.Id} because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(CreateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != Domain.Entities.EditStatus.Both && commitment.EditStatus != Domain.Entities.EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to add apprenticeship {message.Apprenticeship.Id} to commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != Domain.Entities.EditStatus.Both && commitment.EditStatus != Domain.Entities.EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to add apprenticeship {message.Apprenticeship.Id} to commitment {message.CommitmentId}");
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
    }
}
