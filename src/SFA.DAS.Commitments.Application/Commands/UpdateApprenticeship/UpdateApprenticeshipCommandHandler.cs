using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    //todo: add test for UpdateApprenticeshipCommandHandler various scenarios

    public sealed class UpdateApprenticeshipCommandHandler : AsyncRequestHandler<UpdateApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly AbstractValidator<UpdateApprenticeshipCommand> _validator;
        private readonly IApprenticeshipUpdateRules _apprenticeshipUpdateRules;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private HistoryService _historyService;
        
        public UpdateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<UpdateApprenticeshipCommand> validator, IApprenticeshipUpdateRules apprenticeshipUpdateRules, IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger, IHistoryRepository historyRepository)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
            _historyRepository = historyRepository;
        }

        protected override async Task HandleCore(UpdateApprenticeshipCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            CheckAuthorization(command, commitment);
            CheckCommitmentStatus(command, commitment);
            CheckEditStatus(command, commitment);
            CheckPaymentStatus(apprenticeship);

            StartTrackingHistory(commitment, apprenticeship, command.Caller.CallerType, command.UserId, command.UserName);

            UpdateApprenticeshipEntity(apprenticeship, command.Apprenticeship, command);

            await Task.WhenAll(
                _apprenticeshipRepository.UpdateApprenticeship(apprenticeship, command.Caller),
                UpdateStatusOfApprenticeship(commitment, apprenticeship),
                _apprenticeshipEvents.PublishEvent(commitment, apprenticeship, "APPRENTICESHIP-UPDATED"),
                CreateHistory()
            );
        }

        private async Task CreateHistory()
        {
            await _historyService.Save();
        }

        private async Task UpdateStatusOfApprenticeship(Commitment commitment, Apprenticeship updatedApprenticeship)
        {
            foreach (var apprenticeship in commitment.Apprenticeships.Where(x => x.Id != updatedApprenticeship.Id))
            {
                if (apprenticeship.AgreementStatus != updatedApprenticeship.AgreementStatus)
                {
                    await _apprenticeshipRepository.UpdateApprenticeshipStatus(commitment.Id, apprenticeship.Id, updatedApprenticeship.AgreementStatus);
                }
            }
        }

        private void StartTrackingHistory(Commitment commitment, Apprenticeship apprenticeship, CallerType callerType, string userId, string userName)
        {
            _historyService = new HistoryService(_historyRepository);
            _historyService.TrackUpdate(commitment, CommitmentChangeType.EditedApprenticeship.ToString(), commitment.Id, null, callerType, userId, userName);
            _historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.Updated.ToString(), null, apprenticeship.Id, callerType, userId, userName);
        }

        private void LogMessage(UpdateApprenticeshipCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called UpdateApprenticeshipCommand";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, apprenticeshipId: command.ApprenticeshipId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, apprenticeshipId: command.ApprenticeshipId);
        }

        private static void CheckCommitmentStatus(UpdateApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Apprenticeship {message.ApprenticeshipId} in commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(UpdateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to edit apprenticeship {message.ApprenticeshipId} in commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to edit apprenticeship {message.ApprenticeshipId} in commitment {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckPaymentStatus(Apprenticeship apprenticeship)
        {
            var allowedPaymentStatusesForUpdating = new[] {PaymentStatus.Active, PaymentStatus.PendingApproval, PaymentStatus.Paused};

            if (!allowedPaymentStatusesForUpdating.Contains(apprenticeship.PaymentStatus))
                throw new UnauthorizedException($"Apprenticeship {apprenticeship.Id} cannot be updated when payment status is {apprenticeship.PaymentStatus}");
        }

        private static void CheckAuthorization(UpdateApprenticeshipCommand message, Commitment commitment)
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

        private void UpdateApprenticeshipEntity(Apprenticeship existingApprenticeship, Apprenticeship updatedApprenticeship, UpdateApprenticeshipCommand message)
        {
            var doChangesRequireAgreement = _apprenticeshipUpdateRules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship);

            existingApprenticeship.FirstName = updatedApprenticeship.FirstName;
            existingApprenticeship.LastName = updatedApprenticeship.LastName;
            existingApprenticeship.DateOfBirth = updatedApprenticeship.DateOfBirth;
            existingApprenticeship.NINumber = updatedApprenticeship.NINumber;
            existingApprenticeship.ULN = updatedApprenticeship.ULN;
            existingApprenticeship.CommitmentId = message.CommitmentId;
            existingApprenticeship.TrainingType = updatedApprenticeship.TrainingType;
            existingApprenticeship.TrainingCode = updatedApprenticeship.TrainingCode;
            existingApprenticeship.TrainingName = updatedApprenticeship.TrainingName;
            existingApprenticeship.Cost = updatedApprenticeship.Cost;
            existingApprenticeship.StartDate = updatedApprenticeship.StartDate;
            existingApprenticeship.EndDate = updatedApprenticeship.EndDate;
            existingApprenticeship.EmployerRef = updatedApprenticeship.EmployerRef;
            existingApprenticeship.ProviderRef = updatedApprenticeship.ProviderRef;

            existingApprenticeship.AgreementStatus = _apprenticeshipUpdateRules.DetermineNewAgreementStatus(existingApprenticeship.AgreementStatus, message.Caller.CallerType, doChangesRequireAgreement);
            existingApprenticeship.PaymentStatus = _apprenticeshipUpdateRules.DetermineNewPaymentStatus(existingApprenticeship.PaymentStatus, doChangesRequireAgreement);
        }
    }
}
