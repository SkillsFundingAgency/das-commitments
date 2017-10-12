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
    public sealed class ResumeApprenticeshipCommandHandler : AsyncRequestHandler<ResumeApprenticeshipCommand>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly ICurrentDateTime _currentDate;
        private readonly IApprenticeshipEvents _eventsApi;
        private readonly IHistoryRepository _historyRepository;
        private readonly ICommitmentsLogger _logger;
       private readonly ApprenticeshipStatusChangeCommandValidator _validator;

        public ResumeApprenticeshipCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            ApprenticeshipStatusChangeCommandValidator validator,
            ICurrentDateTime currentDate,
            IApprenticeshipEvents eventsApi,
            ICommitmentsLogger logger,
            IHistoryRepository historyRepository)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _currentDate = currentDate;
            _eventsApi = eventsApi;
            _logger = logger;
            _historyRepository = historyRepository;
        }

        protected override async Task HandleCore(ResumeApprenticeshipCommand command)
        {
            _logger.Info($"Employer: {command.AccountId} has called ResumeApprenticeshipCommand", command.AccountId,
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

        private async Task CreateEvent(ResumeApprenticeshipCommand command, Apprenticeship apprenticeship,
            Commitment commitment)
        {
            DateTime effectiveFromDate = command.DateOfChange.Date;

            if (apprenticeship.IsWaitingToStart(_currentDate))
                effectiveFromDate = apprenticeship.StartDate.Value.Date;

            await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship,
                PaymentStatus.Active, effectiveFromDate, null);
        }

        private async Task SaveChange(ResumeApprenticeshipCommand command, Commitment commitment,
            Apprenticeship apprenticeship)
        {
            var historyService = new HistoryService(_historyRepository);

            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.ChangeOfStatus.ToString(),
                apprenticeship.Id, "Apprenticeship", CallerType.Employer, command.UserId, command.UserName);

            await _apprenticeshipRepository.ResumeApprenticeship(commitment.Id, command.ApprenticeshipId);

            apprenticeship.PaymentStatus = PaymentStatus.Active;

            await historyService.Save();
        }

        private void ValidateChangeDate(DateTime dateOfChange)
        {
            if (dateOfChange.Date > _currentDate.Now.Date)
                throw new ValidationException("Invalid Date of Change. Date should be todays date.");
        }


        private static void CheckAuthorization(ResumeApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException(
                    $"Employer {message.AccountId} not authorised to access commitment {commitment.Id}, expected employer {commitment.EmployerAccountId}");
        }
    }
}