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
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
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
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IAcademicYearValidator _academicYearValidator;

        private const DataLockErrorCode CourseChangeErrors = DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock04 | DataLockErrorCode.Dlock05 | DataLockErrorCode.Dlock06;

        public UpdateApprenticeshipStatusCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            UpdateApprenticeshipStatusValidator validator,
            ICurrentDateTime currentDate,
            IApprenticeshipEvents eventsApi,
            ICommitmentsLogger logger,
            IHistoryRepository historyRepository,
            IDataLockRepository dataLockRepository,
            IAcademicYearValidator academicYearValidator
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
            _academicYearValidator = academicYearValidator;
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

            var newPaymentStatus = command.PaymentStatus.GetValueOrDefault(apprenticeship.PaymentStatus);

            await SaveChange(command, commitment, apprenticeship, newPaymentStatus);

            await CreateEvent(command, apprenticeship, commitment, newPaymentStatus);
        }

        private async Task CreateEvent(UpdateApprenticeshipStatusCommand command, Apprenticeship apprenticeship, Commitment commitment, PaymentStatus newPaymentStatus)
        {
            var apprenticeshipHasStarted = apprenticeship.StartDate.Value.Date < _currentDate.Now.Date;
            var resumingApprenticeship = (newPaymentStatus == PaymentStatus.Active && apprenticeship.PauseDate.HasValue);

            if (newPaymentStatus == PaymentStatus.Paused ||
                newPaymentStatus == PaymentStatus.Withdrawn ||
                (resumingApprenticeship && apprenticeshipHasStarted)
            )
            {
                await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship, newPaymentStatus, effectiveFrom: command.DateOfChange.Date);
            }
            else
            {
                await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship, newPaymentStatus, effectiveFrom: apprenticeship.StartDate.Value.Date);
            }
        }

        private async Task SaveChange(UpdateApprenticeshipStatusCommand command, Commitment commitment, Apprenticeship apprenticeship, PaymentStatus newPaymentStatus)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.ChangeOfStatus.ToString(), apprenticeship.Id, "Apprenticeship", CallerType.Employer, command.UserId, command.UserName);
            apprenticeship.PaymentStatus = newPaymentStatus;
            switch (newPaymentStatus)
            {
                case PaymentStatus.Active:
                case PaymentStatus.Paused:
                    ValidateChangeDateForPauseResume(command.DateOfChange);
                    DateTime? pauseDate = (newPaymentStatus == PaymentStatus.Paused
                        ? command.DateOfChange
                        : null as DateTime?);
                    await _apprenticeshipRepository.PauseOrResumeApprenticeship(commitment.Id, command.ApprenticeshipId, newPaymentStatus, pauseDate);
                    break;
                case PaymentStatus.Withdrawn:
                    ValidateChangeDateForStop(command.DateOfChange, apprenticeship);
                    await _apprenticeshipRepository.StopApprenticeship(commitment.Id, command.ApprenticeshipId, command.DateOfChange);
                    await ResolveAnyTriagedCourseDataLocks(command.ApprenticeshipId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newPaymentStatus), "Not a valid value for change of status");
            }
            await historyService.Save();
        }

        private async Task ResolveAnyTriagedCourseDataLocks(long apprenticeshipId)
        {
            var dataLocks = (await _dataLockRepository.GetDataLocks(apprenticeshipId))
                                .Where(x => !x.IsResolved && x.TriageStatus == TriageStatus.Restart && IsCourseChangeError(x.ErrorCode));

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
            if ((errorCode & CourseChangeErrors) > 0)
                return true;

            return false;
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

                if ( apprenticeship.PaymentStatus != PaymentStatus.PendingApproval && 
                    _academicYearValidator.Validate(dateOfChange.Date) == AcademicYearValidationResult.NotWithinFundingPeriod)
                {
                    throw new ValidationException("Invalid Date of Change. Date cannot be before the academic year start date.");
                }
                    
                   
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
                throw new UnauthorizedException($"Employer {message.AccountId} not authorised to access commitment {commitment.Id}, expected employer {commitment.EmployerAccountId}");
        }
    }
}
