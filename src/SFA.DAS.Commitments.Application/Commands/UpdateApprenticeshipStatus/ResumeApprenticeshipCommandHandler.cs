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
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class ResumeApprenticeshipCommandHandler : AsyncRequestHandler<ResumeApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ApprenticeshipStatusChangeCommandValidator _validator;
        private readonly ICurrentDateTime _currentDate;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private readonly IApprenticeshipEvents _eventsApi;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IAcademicYearValidator _academicYearValidator;

        private const DataLockErrorCode CourseChangeErrors = DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock04 | DataLockErrorCode.Dlock05 | DataLockErrorCode.Dlock06;

        public ResumeApprenticeshipCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            ApprenticeshipStatusChangeCommandValidator validator,
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

        protected override async Task HandleCore(ResumeApprenticeshipCommand command)
        {
            _logger.Info($"Employer: {command.AccountId} has called ResumeApprenticeshipCommand", accountId: command.AccountId, apprenticeshipId: command.ApprenticeshipId, caller: command.Caller);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            CheckAuthorization(command, commitment);

            var newPaymentStatus = PaymentStatus.Active;

            await SaveChange(command, commitment, apprenticeship, newPaymentStatus);

            await CreateEvent(command, apprenticeship, commitment, newPaymentStatus);
        }

        private async Task CreateEvent(ResumeApprenticeshipCommand command, Apprenticeship apprenticeship, Commitment commitment, PaymentStatus newPaymentStatus)
        {
            if (apprenticeship.IsWaitingToStart(_currentDate))
            {
                await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship, newPaymentStatus, effectiveFrom: apprenticeship.StartDate.Value.Date);
            }
            else
            {
                await _eventsApi.PublishChangeApprenticeshipStatusEvent(commitment, apprenticeship, newPaymentStatus, effectiveFrom: command.DateOfChange.Date);
            }
        }

        private async Task SaveChange(ResumeApprenticeshipCommand command, Commitment commitment, Apprenticeship apprenticeship, PaymentStatus newPaymentStatus)
        {
            var historyService = new HistoryService(_historyRepository);

            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.ChangeOfStatus.ToString(), apprenticeship.Id, "Apprenticeship", CallerType.Employer, command.UserId, command.UserName);

            apprenticeship.PaymentStatus = newPaymentStatus;

            ValidateChangeDateForPauseResume(command.DateOfChange);

            await _apprenticeshipRepository.PauseOrResumeApprenticeship(commitment.Id, command.ApprenticeshipId, newPaymentStatus, null);

            await historyService.Save();
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

                if (_academicYearValidator.Validate(dateOfChange.Date) == AcademicYearValidationResult.NotWithinFundingPeriod)
                    throw new ValidationException("Invalid Date of Change. Date cannot be before the academic year start date.");
            }
        }

        private void ValidateChangeDateForPauseResume(DateTime dateOfChange)
        {
            if (dateOfChange.Date > _currentDate.Now.Date)
                throw new ValidationException("Invalid Date of Change. Date should be todays date.");
        }

        private static void CheckAuthorization(ResumeApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer {message.AccountId} not authorised to access commitment {commitment.Id}, expected employer {commitment.EmployerAccountId}");
        }
    }
}