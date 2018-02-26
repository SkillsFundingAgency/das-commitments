using System;
using System.Threading.Tasks;
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

        private readonly IAcademicYearValidator _academicYearValidator;

        public UpdateApprenticeshipStopDateCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            UpdateApprenticeshipStopDateCommandValidator validator,
            ICurrentDateTime currentDate,
            ICommitmentsLogger logger,
            IHistoryRepository historyRepository,
            IAcademicYearValidator academicYearValidator
            )
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _currentDate = currentDate;
            _logger = logger;
            _historyRepository = historyRepository;
            _academicYearValidator = academicYearValidator;
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
        }

        private async Task SaveChange(UpdateApprenticeshipStopDateCommand command, Commitment commitment, Apprenticeship apprenticeship)
        {
            var historyService = new HistoryService(_historyRepository);

            historyService.TrackUpdate(apprenticeship, ApprenticeshipChangeType.ChangeOfStopDate.ToString(), commitment.Id, apprenticeship.Id, CallerType.Employer, command.UserId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, command.UserName);
           
            await _apprenticeshipRepository.UpdateApprenticeshipStopDate(commitment.Id, command.ApprenticeshipId, command.StopDate);

            await historyService.Save();
        }

        private void ValidateChangeDateForStop(DateTime newStopDate, Apprenticeship apprenticeship)
        {
            if (apprenticeship == null) throw new ArgumentException(nameof(apprenticeship));

            if (apprenticeship.PaymentStatus != PaymentStatus.Withdrawn)
            {
                throw new ValidationException("The apprenticeship is not stopped so a new stop date cannot be applied.");
            }
          
            if (apprenticeship.IsWaitingToStart(_currentDate))
            {
                if (newStopDate.Date != apprenticeship.StartDate.Value.Date)
                    throw new ValidationException("Invalid Stop Date. Date should be the start date if training has not started.");
            }
            else
            {
                if (newStopDate.Date > _currentDate.Now.Date)
                    throw new ValidationException("Invalid Stop Date. Date cannot be in the future.");

                if ( newStopDate.Date < apprenticeship.StartDate.Value.Date)
                    throw new ValidationException("Invalid Stop Date. Date cannot be before the training start date.");

                if ( apprenticeship.PaymentStatus != PaymentStatus.PendingApproval && 
                    _academicYearValidator.Validate(newStopDate.Date) == AcademicYearValidationResult.NotWithinFundingPeriod)
                {
                    throw new ValidationException("Invalid Stop Date. Date cannot be before the academic year start date.");
                }
            }
        }

        private static void CheckAuthorization(UpdateApprenticeshipStopDateCommand message, Commitment commitment)
        {
            if (commitment.EmployerAccountId != message.AccountId)
                throw new UnauthorizedException($"Employer {message.AccountId} not authorised to access commitment {commitment.Id}, expected employer {commitment.EmployerAccountId}");
        }
    }
}