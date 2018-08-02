using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStopDate
{
    public sealed class UpdateApprenticeshipStopDateCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStopDateCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly UpdateApprenticeshipStopDateCommandValidator _validator;
        private readonly ICurrentDateTime _currentDate;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private readonly IApprenticeshipEventsPublisher _eventsPublisher;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IAcademicYearValidator _academicYearValidator;

        public UpdateApprenticeshipStopDateCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            UpdateApprenticeshipStopDateCommandValidator validator,
            ICurrentDateTime currentDate,
            ICommitmentsLogger logger,
            IHistoryRepository historyRepository,
            IAcademicYearValidator academicYearValidator,
            IApprenticeshipEventsPublisher eventsPublisher,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IDataLockRepository dataLockRepository)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _currentDate = currentDate;
            _logger = logger;
            _historyRepository = historyRepository;
            _academicYearValidator = academicYearValidator;
            _eventsPublisher = eventsPublisher;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _dataLockRepository = dataLockRepository;
        }

        protected override async Task HandleCore(UpdateApprenticeshipStopDateCommand command)
        {
            _logger.Info($"Employer: {command.AccountId} has called StopApprenticeshipCommand", command.AccountId, apprenticeshipId: command.ApprenticeshipId, caller: command.Caller);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            CheckAuthorization(command, commitment);

            ValidateChangeDateForStop(command.StopDate, apprenticeship);

            await SaveChange(command, commitment, apprenticeship);

            if (command.StopDate == apprenticeship.StartDate)
            {
                await ResolveDataLocksForApprenticeship(apprenticeship.Id);
            }

            await PublishEvent(command, commitment, apprenticeship);
        }

        private async Task ResolveDataLocksForApprenticeship(long apprenticeshipId)
        {
            var apprenticeshipDataLocks = (await _dataLockRepository.GetDataLocks(apprenticeshipId)).Select(x=>x.DataLockEventId);
            await _dataLockRepository.ResolveDataLock(apprenticeshipDataLocks);
        }

        private async Task SaveChange(UpdateApprenticeshipStopDateCommand command, Commitment commitment, Apprenticeship apprenticeship)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.ChangeOfStopDate.ToString(), null, apprenticeship.Id, CallerType.Employer, command.UserId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, command.UserName);
           
            await _apprenticeshipRepository.UpdateApprenticeshipStopDate(commitment.Id, command.ApprenticeshipId, command.StopDate);
            apprenticeship.StopDate = command.StopDate;

            await historyService.Save();
        }

        private async Task PublishEvent(UpdateApprenticeshipStopDateCommand command, Commitment commitment, Apprenticeship apprenticeship)
        {
            _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-UPDATED", command.StopDate);
            await _eventsPublisher.Publish(_apprenticeshipEventsList);
        }

        private void ValidateChangeDateForStop(DateTime newStopDate, Apprenticeship apprenticeship)
        {
            if (apprenticeship == null) throw new ArgumentException(nameof(apprenticeship));

            if (apprenticeship.PaymentStatus != PaymentStatus.Withdrawn)
            {
                throw new ValidationException("Apprenticeship must be stopped in order to update stop date");
            }

            if (newStopDate.Date > _currentDate.Now.Date)
                throw new ValidationException("Invalid Date of Change. Date cannot be in the future.");

            if (newStopDate.Date >= apprenticeship.StopDate)
                throw new ValidationException("Invalid Date of Change. Date must be before current stop date.");

            if (newStopDate.Date < apprenticeship.StartDate.Value.Date)
                throw new ValidationException("Invalid Date of Change. Date cannot be before the training start date.");

            if ( apprenticeship.PaymentStatus != PaymentStatus.PendingApproval && 
                _academicYearValidator.Validate(newStopDate.Date) == AcademicYearValidationResult.NotWithinFundingPeriod)
            {
                throw new ValidationException("Invalid Date of Change. Date cannot be before the academic year start date.");
            }
            
        }

        private static void CheckAuthorization(UpdateApprenticeshipStopDateCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer {message.AccountId} not authorised to access commitment {commitment.Id}, expected employer {commitment.EmployerAccountId}");
        }
    }
}