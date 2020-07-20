using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class StopApprenticeshipCommandHandler : AsyncRequestHandler<StopApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ApprenticeshipStatusChangeCommandValidator _validator;
        private readonly ICurrentDateTime _currentDate;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private readonly IApprenticeshipEvents _eventsApi;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IV2EventsPublisher _v2EventsPublisher;

        private const DataLockErrorCode CourseChangeErrors = DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock04 | DataLockErrorCode.Dlock05 | DataLockErrorCode.Dlock06;

        public StopApprenticeshipCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            ApprenticeshipStatusChangeCommandValidator validator,
            ICurrentDateTime currentDate,
            IApprenticeshipEvents eventsApi,
            ICommitmentsLogger logger,
            IHistoryRepository historyRepository,
            IDataLockRepository dataLockRepository,
            IV2EventsPublisher v2EventsPublisher
            )
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _currentDate = currentDate;
            _eventsApi = eventsApi;
            _logger = logger;
            _historyRepository = historyRepository;
            _dataLockRepository = dataLockRepository;
            _v2EventsPublisher = v2EventsPublisher;
        }

        protected override async Task HandleCore(StopApprenticeshipCommand command)
        {
            _logger.Info($"Employer: {command.AccountId} has called StopApprenticeshipCommand", accountId: command.AccountId, apprenticeshipId: command.ApprenticeshipId, caller: command.Caller);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            CheckAuthorization(command, commitment);

            ValidateChangeDateForStop(command.DateOfChange, apprenticeship);

            await SaveChange(command, commitment, apprenticeship);

            await CreateEvent(command, apprenticeship, commitment);
        }

        private Task CreateEvent(StopApprenticeshipCommand command, Apprenticeship apprenticeship, Commitment commitment)
        {
            var tasks = new[]
            {
                _eventsApi.PublishChangeApprenticeshipStatusEvent(
                    commitment, apprenticeship,
                    apprenticeship.PaymentStatus,
                    command.DateOfChange.Date),

                _v2EventsPublisher.PublishApprenticeshipStopped(commitment, apprenticeship)
            };

            return Task.WhenAll(tasks);
        }

        private async Task SaveChange(StopApprenticeshipCommand command, Commitment commitment, Apprenticeship apprenticeship)
        {
            var historyService = new HistoryService(_historyRepository);

            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.ChangeOfStatus.ToString(), null, apprenticeship.Id, CallerType.Employer, command.UserId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, command.UserName);
            apprenticeship.PaymentStatus = PaymentStatus.Withdrawn;
            apprenticeship.StopDate = command.DateOfChange;
            apprenticeship.MadeRedundant = command.MadeRedundant;

            await _apprenticeshipRepository.StopApprenticeship(commitment.Id, command.ApprenticeshipId, command.DateOfChange, command.MadeRedundant);

            if (command.DateOfChange == apprenticeship.StartDate)
            {
                await ResolveDataLocksForApprenticeship(apprenticeship.Id);
            }
            else
            {
                await ResolveAnyTriagedCourseDataLocks(command.ApprenticeshipId);
            }

            await historyService.Save();
        }

        private async Task ResolveDataLocksForApprenticeship(long apprenticeshipId)
        {
            var apprenticeshipDataLocks = (await _dataLockRepository.GetDataLocks(apprenticeshipId)).Select(x => x.DataLockEventId);
            await _dataLockRepository.ResolveDataLock(apprenticeshipDataLocks);
        }

        private async Task ResolveAnyTriagedCourseDataLocks(long apprenticeshipId)
        {
            var dataLocks = (await _dataLockRepository.GetDataLocks(apprenticeshipId))
                                .Where(x => !x.IsResolved && x.TriageStatus == TriageStatus.Restart && IsCourseChangeError(x.ErrorCode)).ToList();

            if (dataLocks.Any())
            {
                if (dataLocks.Count() > 1)
                {
                    _logger.Debug($"More than one unresolved data lock with triage status of reset found when stopping apprenticeship. ApprenticeshipId: {apprenticeshipId}", apprenticeshipId);
                }

                foreach (var dataLock in dataLocks)
                {
                    dataLock.IsResolved = true;
                    await _dataLockRepository.UpdateDataLockStatus(dataLock);
                }
            }
        }

        private bool IsCourseChangeError(DataLockErrorCode errorCode)
        {
            return (errorCode & CourseChangeErrors) > 0;
        }

        private void ValidateChangeDateForStop(DateTime dateOfChange, Apprenticeship apprenticeship)
        {
            if (apprenticeship == null) throw new ArgumentException(nameof(apprenticeship));

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

        private static void CheckAuthorization(StopApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer {message.AccountId} not authorised to access commitment {commitment.Id}, expected employer {commitment.EmployerAccountId}");
        }
    }
}
