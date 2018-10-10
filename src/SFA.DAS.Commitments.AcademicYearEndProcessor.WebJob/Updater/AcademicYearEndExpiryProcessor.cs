using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater
{
    public class AcademicYearEndExpiryProcessor : IAcademicYearEndExpiryProcessor
    {
        private readonly ILog _logger;
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IMessagePublisher _messagePublisher;

        public AcademicYearEndExpiryProcessor(
            ILog logger, 
            IAcademicYearDateProvider academicYearProvider, 
            IDataLockRepository dataLockRepository,
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository,
            ICurrentDateTime currentDateTime,
            IMessagePublisher messagePublisher)
        {
            _logger = logger;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _currentDateTime = currentDateTime;
            _messagePublisher = messagePublisher;
            _academicYearProvider = academicYearProvider;
        }

        public async Task RunDataLock(string jobId)
        {
            _logger.Info($"{nameof(AcademicYearEndExpiryProcessor)} run at {_currentDateTime.Now} for Academic Year CurrentAcademicYearStartDate: {_academicYearProvider.CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {_academicYearProvider.CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {_academicYearProvider.LastAcademicYearFundingPeriod}, JobId: {jobId}");

            var expirableDatalocks = await _dataLockRepository.GetExpirableDataLocks(_academicYearProvider.CurrentAcademicYearStartDate);

            foreach (var expirableDatalock in expirableDatalocks)
            {
                _logger.Info($"Updating DataLockStatus for apprenticeshipId: {expirableDatalock.ApprenticeshipId} and PriceEpisodeIdentifier: {expirableDatalock.ApprenticeshipId}, JobId: {jobId}");
                await _dataLockRepository.UpdateExpirableDataLocks(expirableDatalock.ApprenticeshipId,
                    expirableDatalock.PriceEpisodeIdentifier, _currentDateTime.Now);
            }
            _logger.Info($"{nameof(AcademicYearEndExpiryProcessor)} expired {expirableDatalocks.Count} items, JobId: {jobId}");
        }

        public async Task RunApprenticeshipUpdateJob(string jobId)
        {
            _logger.Info($"{nameof(AcademicYearEndExpiryProcessor)} run at {_currentDateTime.Now} for Academic Year CurrentAcademicYearStartDate: {_academicYearProvider.CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {_academicYearProvider.CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {_academicYearProvider.LastAcademicYearFundingPeriod}, JobId: {jobId}");


            var expiredApprenticeshipUpdates = 
                ( await _apprenticeshipUpdateRepository
                .GetExpiredApprenticeshipUpdates(_academicYearProvider.CurrentAcademicYearStartDate))
                .Where(m => m.Cost != null || m.TrainingCode != null || m.StartDate != null)
                .ToArray();


            _logger.Info($"Found {expiredApprenticeshipUpdates.Length} apprenticeship updates that will be set to expired, JobId: {jobId}");

            foreach (var update in expiredApprenticeshipUpdates)
            {
                _logger.Info($"Updating ApprenticeshipUpdate to expired, ApprenticeshipUpdateId: {update.Id}, JobId: {jobId}");
                await _apprenticeshipUpdateRepository.ExpireApprenticeshipUpdate(update.Id);

                await _messagePublisher.PublishAsync(
                    new ApprenticeshipUpdateCancelled(1, 1,
                        1)); //update.EmployerRef, update.ProviderRef, update.ApprenticeshipId));
                //todo send msg to task q
                // task is to raise new ApprenticeshipUpdateCancelled event and put onto new task bus
                // does task bus have api? client nuget?
            }

            var expiredApprenticeshipUpdatesAfterJob =
                (await _apprenticeshipUpdateRepository
                .GetExpiredApprenticeshipUpdates(_academicYearProvider.CurrentAcademicYearStartDate))
                .Where(m => m.Cost != null || m.TrainingCode != null || m.StartDate != null)
                .ToArray();

            if (expiredApprenticeshipUpdatesAfterJob.Length != 0)
            {
                throw new Exception($"AcademicYearEndProcessor not completed successfull, Should not be any pending ApprenticeshipUpdates after job done, There are {expiredApprenticeshipUpdatesAfterJob.Length} , JobId: {jobId}");
            }
        }
    }
}