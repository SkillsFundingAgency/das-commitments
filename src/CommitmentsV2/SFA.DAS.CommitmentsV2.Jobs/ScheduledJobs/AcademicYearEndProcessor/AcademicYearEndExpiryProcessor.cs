using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Events;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob.Updater
{
    public class AcademicYearEndExpiryProcessor : IAcademicYearEndExpiryProcessor
    {
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<AcademicYearEndExpiryProcessor> _logger;

        public AcademicYearEndExpiryProcessor(ILogger<AcademicYearEndExpiryProcessor> logger,
            IAcademicYearDateProvider academicYearProvider,
            IDataLockRepository dataLockRepository,
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository,
            ICurrentDateTime currentDateTime,
            IEventPublisher eventPublisher,
            IApprenticeshipRepository apprenticeshipRepository)
        {
            _logger = logger;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _currentDateTime = currentDateTime;
            _eventPublisher = eventPublisher;
            _apprenticeshipRepository = apprenticeshipRepository;
            _academicYearProvider = academicYearProvider;
        }

        public async Task RunDataLock(string jobId)
        {
            _logger.LogInformation($"{nameof(AcademicYearEndExpiryProcessor)} run at {_currentDateTime.UtcNow} for Academic Year CurrentAcademicYearStartDate: {_academicYearProvider.CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {_academicYearProvider.CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {_academicYearProvider.LastAcademicYearFundingPeriod}, JobId: {jobId}");

            var expirableDatalocks = await _dataLockRepository.GetExpirableDataLocks(_academicYearProvider.CurrentAcademicYearStartDate);

            foreach (var expirableDatalock in expirableDatalocks)
            {
                _logger.LogInformation($"Updating DataLockStatus for apprenticeshipId: {expirableDatalock.ApprenticeshipId} and PriceEpisodeIdentifier: {expirableDatalock.ApprenticeshipId}, JobId: {jobId}");

                await _dataLockRepository.UpdateExpirableDataLocks(expirableDatalock);
            }
            _logger.LogInformation($"{nameof(AcademicYearEndExpiryProcessor)} expired {expirableDatalocks.Count} items, JobId: {jobId}");
        }

        public async Task RunApprenticeshipUpdateJob(string jobId)
        {
            _logger.LogInformation($"{nameof(AcademicYearEndExpiryProcessor)} run at {_currentDateTime.UtcNow} for Academic Year CurrentAcademicYearStartDate: {_academicYearProvider.CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {_academicYearProvider.CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {_academicYearProvider.LastAcademicYearFundingPeriod}, JobId: {jobId}");


            var expiredApprenticeshipUpdates =
                (await _apprenticeshipUpdateRepository
                .GetExpiredApprenticeshipUpdates(_academicYearProvider.CurrentAcademicYearStartDate))
                .Where(m => m.Cost != null || m.TrainingCode != null || m.StartDate != null)
                .ToArray();

            _logger.LogInformation($"Found {expiredApprenticeshipUpdates.Length} apprenticeship updates that will be set to expired, JobId: {jobId}");

            foreach (var update in expiredApprenticeshipUpdates)
            {
                _logger.LogInformation($"Updating ApprenticeshipUpdate to expired, ApprenticeshipUpdateId: {update.Id}, JobId: {jobId}");

                await _apprenticeshipUpdateRepository.ExpireApprenticeshipUpdate(update);

                var apprenticeship =
                    await _apprenticeshipRepository.GetApprenticeship(update.ApprenticeshipId);

                await _eventPublisher.Publish(new ApprenticeshipUpdateCancelled(
                    apprenticeship.EmployerAccountId,
                    apprenticeship.ProviderId,
                    apprenticeship.Id));
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