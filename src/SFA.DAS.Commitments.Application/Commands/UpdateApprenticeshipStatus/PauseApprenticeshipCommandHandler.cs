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
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly ICurrentDateTime _currentDate;
        private readonly IApprenticeshipEvents _eventsApi;
        private readonly IHistoryRepository _historyRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly ApprenticeshipStatusChangeCommandValidator _validator;

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
            _logger.Info($"Employer: {command.AccountId} has called PauseApprenticeshipCommand", command.AccountId,
                apprenticeshipId: command.ApprenticeshipId, caller: command.Caller);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            CheckAuthorization(command, commitment);

            ValidateChangeDate(command.DateOfChange);


            await SaveChange(command, commitment, apprenticeship);

            await CreateEvent(command, apprenticeship, commitment);
        }

        private async Task CreateEvent(PauseApprenticeshipCommand command, Apprenticeship apprenticeship,
            Commitment commitment)
        {
            await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship, PaymentStatus.Paused,
                command.DateOfChange.Date);
        }

        private async Task SaveChange(PauseApprenticeshipCommand command, Commitment commitment,
            Apprenticeship apprenticeship)
        {
            var historyService = new HistoryService(_historyRepository);

            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.ChangeOfStatus.ToString(), 
                null, apprenticeship.Id, CallerType.Employer, command.UserId, command.UserName);
          
            await _apprenticeshipRepository.PauseApprenticeship(commitment.Id, command.ApprenticeshipId, command.DateOfChange);

            apprenticeship.PaymentStatus = PaymentStatus.Paused;

            await historyService.Save();
        }

        private void ValidateChangeDate(DateTime dateOfChange)
        {
            if (dateOfChange.Date > _currentDate.Now.Date)
                throw new ValidationException("Invalid Date of Change. Date should be todays date.");
        }

        private static void CheckAuthorization(PauseApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException(
                    $"Employer {message.AccountId} not authorised to access commitment {commitment.Id}, expected employer {commitment.EmployerAccountId}");
        }
    }
}