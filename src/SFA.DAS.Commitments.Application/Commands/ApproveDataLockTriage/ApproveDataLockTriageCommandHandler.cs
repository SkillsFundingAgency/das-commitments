using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.ApproveDataLockTriage
{
    public class ApproveDataLockTriageCommandHandler : AsyncRequestHandler<ApproveDataLockTriageCommand>
    {
        private readonly AbstractValidator<ApproveDataLockTriageCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _eventsApi;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IApprenticeshipInfoService _apprenticeshipTrainingService;
        private readonly IV2EventsPublisher _v2EventsPublisher;

        private readonly ICommitmentsLogger _logger;

        public ApproveDataLockTriageCommandHandler(AbstractValidator<ApproveDataLockTriageCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipEventsPublisher eventsApi,
            IApprenticeshipEventsList apprenticeshipEventsList,
            ICommitmentRepository commitmentRepository, 
            ICurrentDateTime currentDateTime,
            IApprenticeshipInfoService apprenticeshipTrainingService,
            ICommitmentsLogger logger,
            IV2EventsPublisher v2EventsPublisher)
        {
            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _eventsApi = eventsApi;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _commitmentRepository = commitmentRepository;
            _currentDateTime = currentDateTime;
            _apprenticeshipTrainingService = apprenticeshipTrainingService;
            _v2EventsPublisher = v2EventsPublisher;
            _logger = logger;
        }

        protected override async Task HandleCore(ApproveDataLockTriageCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var datalocksForApprenticeship = await _dataLockRepository.GetDataLocks(command.ApprenticeshipId);

            var dataLocksToBeUpdated = datalocksForApprenticeship
                .Where(DataLockExtensions.UnHandled)
                .Where(m => m.TriageStatus == TriageStatus.Change);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);
            if (apprenticeship.HasHadDataLockSuccess)
            {
                dataLocksToBeUpdated = dataLocksToBeUpdated.Where(DataLockExtensions.IsPriceOnly);
            }

            var dataLockPasses = datalocksForApprenticeship.Where(x => x.Status == Status.Pass || x.PreviousResolvedPriceDataLocks() );

            if (!dataLocksToBeUpdated.Any())
                return;

            var newPriceHistory = CreatePriceHistory(command, dataLocksToBeUpdated, dataLockPasses);

            await _apprenticeshipRepository.InsertPriceHistory(command.ApprenticeshipId, newPriceHistory);
            apprenticeship.PriceHistory = newPriceHistory.ToList();

            if (!apprenticeship.HasHadDataLockSuccess)
            {
                var dataLockWithUpdatedTraining = dataLocksToBeUpdated.FirstOrDefault(m => m.IlrTrainingCourseCode != apprenticeship.TrainingCode);
                if (dataLockWithUpdatedTraining != null)
                {
                    var training = await
                        _apprenticeshipTrainingService.GetTrainingProgram(dataLockWithUpdatedTraining.IlrTrainingCourseCode);

                    _logger.Info($"Updating course for apprenticeship {apprenticeship.Id} from training code {apprenticeship.TrainingCode} to {dataLockWithUpdatedTraining.IlrTrainingCourseCode}");

                    apprenticeship.TrainingCode = dataLockWithUpdatedTraining.IlrTrainingCourseCode;
                    apprenticeship.TrainingName = training.Title;
                    apprenticeship.TrainingType = dataLockWithUpdatedTraining.IlrTrainingType;
                    await _apprenticeshipRepository.UpdateApprenticeship(apprenticeship, command.Caller);
                }
            }

            await _dataLockRepository.ResolveDataLock(dataLocksToBeUpdated.Select(m => m.DataLockEventId));
            
            await PublishEvents(apprenticeship);
        }

        private static PriceHistory[] CreatePriceHistory(
            ApproveDataLockTriageCommand command,
            IEnumerable<DataLockStatus> dataLocksToBeUpdated,
            IEnumerable<DataLockStatus> dataLockPasses)
        {
            var newPriceHistory =
                dataLocksToBeUpdated.Concat(dataLockPasses)
                    .Select(
                        m =>
                        new PriceHistory
                            {
                                ApprenticeshipId = command.ApprenticeshipId,
                                Cost = (decimal)m.IlrTotalCost,
                                FromDate = (DateTime)m.IlrEffectiveFromDate,
                                ToDate = null
                            })
                    .OrderBy(x => x.FromDate)
                    .ToArray();

            for (var i = 0; i < newPriceHistory.Length - 1; i++)
            {
                var derivedToDate = newPriceHistory[i + 1].FromDate.AddDays(-1);

                newPriceHistory[i].ToDate = derivedToDate < newPriceHistory[i].FromDate
                    ? newPriceHistory[i].FromDate
                    : derivedToDate;
            }
            return newPriceHistory;
        }

        private async Task PublishEvents(Apprenticeship apprenticeship)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-UPDATED", _currentDateTime.Now);

            var tasks = _apprenticeshipEventsList.Events.Select(x => _v2EventsPublisher.PublishDataLockTriageApproved(x));
            await Task.WhenAll(tasks);

            await _eventsApi.Publish(_apprenticeshipEventsList);
        }
    }
}
