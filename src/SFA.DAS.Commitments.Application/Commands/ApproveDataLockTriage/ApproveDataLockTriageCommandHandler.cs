using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

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
        private readonly IApprenticeshipInfoServiceWrapper _apprenticeshipTrainingService;

        private readonly ICommitmentsLogger _logger;
        private readonly IMessagePublisher _messagePublisher;

        public ApproveDataLockTriageCommandHandler(AbstractValidator<ApproveDataLockTriageCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipEventsPublisher eventsApi,
            IApprenticeshipEventsList apprenticeshipEventsList,
            ICommitmentRepository commitmentRepository, 
            ICurrentDateTime currentDateTime,
            IApprenticeshipInfoServiceWrapper apprenticeshipTrainingService,
            ICommitmentsLogger logger,
            IMessagePublisher messagePublisher)
        {
            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _eventsApi = eventsApi;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _commitmentRepository = commitmentRepository;
            _currentDateTime = currentDateTime;
            _apprenticeshipTrainingService = apprenticeshipTrainingService;
            _logger = logger;
            _messagePublisher = messagePublisher;
        }

        protected override async Task HandleCore(ApproveDataLockTriageCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            var datalocksForApprenticeship = await _dataLockRepository.GetDataLocks(command.ApprenticeshipId);

            var dataLocksToBeUpdated = GetDataLocksToBeUpdated(datalocksForApprenticeship, apprenticeship);
            if (!dataLocksToBeUpdated.Any())
                return;

            var dataLockPasses = datalocksForApprenticeship.Where(x => x.Status == Status.Pass || x.PreviousResolvedPriceDataLocks() );

            await UpdatePriceHistory(command, dataLocksToBeUpdated, dataLockPasses, apprenticeship);

            if (!apprenticeship.HasHadDataLockSuccess)
            {
                await UpdateTraining(command, dataLocksToBeUpdated, apprenticeship);
            }

            await _dataLockRepository.ResolveDataLock(dataLocksToBeUpdated.Select(m => m.DataLockEventId));
            
            await PublishEvents(apprenticeship);
        }

        private async Task UpdateTraining(ApproveDataLockTriageCommand command, IEnumerable<DataLockStatus> dataLocksToBeUpdated, Apprenticeship apprenticeship)
        {
            var dataLockWithUpdatedTraining = dataLocksToBeUpdated.FirstOrDefault(m => m.IlrTrainingCourseCode != apprenticeship.TrainingCode);
            if (dataLockWithUpdatedTraining != null)
            {
                var training = await
                    _apprenticeshipTrainingService.GetTrainingProgramAsync(dataLockWithUpdatedTraining.IlrTrainingCourseCode);

                _logger.Info($"Updating course for apprenticeship {apprenticeship.Id} from training code {apprenticeship.TrainingCode} to {dataLockWithUpdatedTraining.IlrTrainingCourseCode}");

                apprenticeship.TrainingCode = dataLockWithUpdatedTraining.IlrTrainingCourseCode;
                apprenticeship.TrainingName = training.Title;
                apprenticeship.TrainingType = dataLockWithUpdatedTraining.IlrTrainingType;
                await _apprenticeshipRepository.UpdateApprenticeship(apprenticeship, command.Caller);
            }
        }

        private async Task UpdatePriceHistory(ApproveDataLockTriageCommand command, IEnumerable<DataLockStatus> dataLocksToBeUpdated, IEnumerable<DataLockStatus> dataLockPasses, Apprenticeship apprenticeship)
        {
            var newPriceHistory = CreatePriceHistory(command, dataLocksToBeUpdated, dataLockPasses);

            await _apprenticeshipRepository.InsertPriceHistory(command.ApprenticeshipId, newPriceHistory);
            apprenticeship.PriceHistory = newPriceHistory.ToList();
        }

        private static IEnumerable<DataLockStatus> GetDataLocksToBeUpdated(List<DataLockStatus> datalocksForApprenticeship, Apprenticeship apprenticeship)
        {
            var dataLockService = new DataLockTriageService();
            return dataLockService.GetDataLocksToBeUpdated(datalocksForApprenticeship, apprenticeship);
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
                newPriceHistory[i].ToDate = newPriceHistory[i + 1].FromDate.AddDays(-1);
            }
            return newPriceHistory;
        }

        private async Task PublishEvents(Apprenticeship apprenticeship)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-UPDATED", _currentDateTime.Now);

            await _eventsApi.Publish(_apprenticeshipEventsList);
            await _messagePublisher.PublishAsync(new DataLockTriageApproved(apprenticeship.EmployerAccountId, apprenticeship.ProviderId, apprenticeship.Id));
        }
    }
}
