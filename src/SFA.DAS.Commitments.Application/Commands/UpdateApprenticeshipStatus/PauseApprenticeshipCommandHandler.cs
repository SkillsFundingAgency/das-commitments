using System;
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

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class PauseApprenticeshipCommandHandler : AsyncRequestHandler<PauseApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ApprenticeshipStatusChangeCommandValidator _validator;
        private readonly ICurrentDateTime _currentDate;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private readonly IApprenticeshipEvents _eventsApi;
      
        public PauseApprenticeshipCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            ApprenticeshipStatusChangeCommandValidator validator,
            ICurrentDateTime currentDate,
            IApprenticeshipEvents eventsApi,
            ICommitmentsLogger logger,
            IHistoryRepository historyRepository
        )
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _currentDate = currentDate;
            _eventsApi = eventsApi;
            _logger = logger;
            _historyRepository = historyRepository;
        }

        protected override async Task HandleCore(PauseApprenticeshipCommand command)
        {
            _logger.Info($"Employer: {command.AccountId} has called PauseApprenticeshipCommand", accountId: command.AccountId, apprenticeshipId: command.ApprenticeshipId, caller: command.Caller);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            CheckAuthorization(command, commitment);

            var newPaymentStatus = PaymentStatus.Paused;

            await SaveChange(command, commitment, apprenticeship, newPaymentStatus);

            await CreateEvent(command, apprenticeship, commitment, newPaymentStatus);
        }

        private async Task CreateEvent(PauseApprenticeshipCommand command, Apprenticeship apprenticeship, Commitment commitment, PaymentStatus newPaymentStatus)
        {

            await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship, newPaymentStatus, effectiveFrom: command.DateOfChange.Date);

        }

        private async Task SaveChange(PauseApprenticeshipCommand command, Commitment commitment, Apprenticeship apprenticeship, PaymentStatus newPaymentStatus)
        {
            var historyService = new HistoryService(_historyRepository);

            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.ChangeOfStatus.ToString(), apprenticeship.Id, "Apprenticeship", CallerType.Employer, command.UserId, command.UserName);

            apprenticeship.PaymentStatus = newPaymentStatus;

            ValidateChangeDateForPauseResume(command.DateOfChange);

            await _apprenticeshipRepository.PauseOrResumeApprenticeship(commitment.Id, command.ApprenticeshipId, newPaymentStatus, command.DateOfChange);

            await historyService.Save();
        }

        private void ValidateChangeDateForPauseResume(DateTime dateOfChange)
        {
            if (dateOfChange.Date > _currentDate.Now.Date)
                throw new ValidationException("Invalid Date of Change. Date should be todays date.");
        }

        private static void CheckAuthorization(PauseApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer {message.AccountId} not authorised to access commitment {commitment.Id}, expected employer {commitment.EmployerAccountId}");
        }
    }
}