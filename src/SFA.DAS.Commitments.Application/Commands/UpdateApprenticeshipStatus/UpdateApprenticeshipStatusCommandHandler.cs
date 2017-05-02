using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStatusCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly UpdateApprenticeshipStatusValidator _validator;
        private readonly ICurrentDateTime _currentDate;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private readonly IApprenticeshipEvents _eventsApi;

        public UpdateApprenticeshipStatusCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, UpdateApprenticeshipStatusValidator validator, ICurrentDateTime currentDate, IApprenticeshipEvents eventsApi, ICommitmentsLogger logger, IHistoryRepository historyRepository)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _currentDate = currentDate;
            _eventsApi = eventsApi;
            _logger = logger;
            _historyRepository = historyRepository;
        }

        protected override async Task HandleCore(UpdateApprenticeshipStatusCommand command)
        {
            _logger.Info($"Employer: {command.AccountId} has called UpdateApprenticeshipStatusCommand", accountId: command.AccountId, apprenticeshipId: command.ApprenticeshipId);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);
            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);
            CheckAuthorization(command, commitment);

            var newPaymentStatus = (PaymentStatus)command.PaymentStatus.GetValueOrDefault((Api.Types.Apprenticeship.Types.PaymentStatus)apprenticeship.PaymentStatus);

            await SaveChange(command, commitment, apprenticeship, newPaymentStatus);

            await CreateEvent(command, apprenticeship, commitment, newPaymentStatus);
        }

        private async Task CreateEvent(UpdateApprenticeshipStatusCommand command, Apprenticeship apprenticeship, Commitment commitment, PaymentStatus newPaymentStatus)
        {
            if (newPaymentStatus == PaymentStatus.Withdrawn || newPaymentStatus == PaymentStatus.Paused)
                await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship, newPaymentStatus, effectiveTo: command.DateOfChange.Date);
            else
                await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship, newPaymentStatus, effectiveFrom: apprenticeship.StartDate.Value.Date);
        }

        private async Task SaveChange(UpdateApprenticeshipStatusCommand command, Commitment commitment, Apprenticeship apprenticeship, PaymentStatus newPaymentStatus)
        {
            var historyService = new HistoryService(_historyRepository, apprenticeship, ApprenticeshipChangeType.ChangeOfStatus.ToString(), apprenticeship.Id, "Apprenticeship", CallerType.Employer, command.UserId);
            apprenticeship.PaymentStatus = newPaymentStatus;
            switch (newPaymentStatus)
            {
                case PaymentStatus.Active:
                case PaymentStatus.Paused:
                    ValidateChangeDateForPauseResume(command.DateOfChange);
                    await _apprenticeshipRepository.PauseOrResumeApprenticeship(commitment.Id, command.ApprenticeshipId, newPaymentStatus);
                    break;
                case PaymentStatus.Withdrawn:
                    ValidateChangeDateForStop(command.DateOfChange, apprenticeship);
                    await _apprenticeshipRepository.StopApprenticeship(commitment.Id, command.ApprenticeshipId, command.DateOfChange);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newPaymentStatus), "Not a valid value for change of status");
            }
            await historyService.CreateUpdate();
        }

        private void ValidateChangeDateForStop(DateTime dateOfChange, Apprenticeship apprenticeship)
        {
            if (apprenticeship.IsWaitingToStart(_currentDate))
            {
                if (dateOfChange.Date != apprenticeship.StartDate.Value.Date)
                    throw new ValidationException("Invalid Date of Change. Date should be value of start date if training has not started.");
            }
            else
            {
                if (dateOfChange.Date > _currentDate.Now.Date)
                    throw new ValidationException("Invalid Date of Change. Date cannot be in the future.");

                if (dateOfChange.Date < apprenticeship.StartDate.Value.Date)
                    throw new ValidationException("Invalid Date of Change. Date cannot be before the training start date.");
            }
        }

        private void ValidateChangeDateForPauseResume(DateTime dateOfChange)
        {
            if (dateOfChange.Date > _currentDate.Now.Date)
                throw new ValidationException("Invalid Date of Change. Date should be todays date.");
        }

        private static void CheckAuthorization(UpdateApprenticeshipStatusCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer {message.AccountId} unauthorized to view commitment {commitment.Id}");
        }
    }
}
