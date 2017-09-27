using System;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
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

        public AcademicYearEndExpiryProcessor(
            ILog logger, 
            IAcademicYearDateProvider academicYearProvider, 
            IDataLockRepository dataLockRepository,
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository,
            ICurrentDateTime currentDateTime)
        {

            if (logger == null) throw new ArgumentException(nameof(logger));
            if (dataLockRepository == null) throw new ArgumentException(nameof(dataLockRepository));
            if (currentDateTime == null) throw new ArgumentException(nameof(currentDateTime));
            if (academicYearProvider == null) throw new ArgumentException(nameof(academicYearProvider));
            if (apprenticeshipUpdateRepository== null) throw new ArgumentException(nameof(apprenticeshipUpdateRepository));


            _logger = logger;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _currentDateTime = currentDateTime;
            _academicYearProvider = academicYearProvider;
        }

        public async Task RunUpdate()
        {
            // ToDo: Improve logging. 
            // ToDo: Do we want to guard agains running it too early?
            _logger.Info($"{nameof(AcademicYearEndExpiryProcessor)} run at {_currentDateTime.Now} for Academic Year CurrentAcademicYearStartDate: {_academicYearProvider.CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {_academicYearProvider.CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {_academicYearProvider.LastAcademicYearFundingPeriod}");

            if (_currentDateTime.Now >= _academicYearProvider.LastAcademicYearFundingPeriod)
            {
                var expirableDatalocks = await _dataLockRepository.GetExpirableDataLocks(_academicYearProvider.CurrentAcademicYearStartDate);

                foreach (var expirableDatalock in expirableDatalocks)
                {
                    await _dataLockRepository.UpdateExpirableDataLocks(expirableDatalock.ApprenticeshipId,
                        expirableDatalock.PriceEpisodeIdentifier, _currentDateTime.Now);
                }
                _logger.Info($"{nameof(AcademicYearEndExpiryProcessor)} expired {expirableDatalocks.Count} items");
            }
            else
            {
                _logger.Info($"{nameof(AcademicYearEndExpiryProcessor)} please run after {_academicYearProvider.LastAcademicYearFundingPeriod}");
            }
            

        }

        public async Task RunChangeOfCircUpdate()
        {
            _logger.Info($"{nameof(AcademicYearEndExpiryProcessor)} run at {_currentDateTime.Now} for Academic Year CurrentAcademicYearStartDate: {_academicYearProvider.CurrentAcademicYearStartDate}, CurrentAcademicYearEndDate: {_academicYearProvider.CurrentAcademicYearEndDate}, LastAcademicYearFundingPeriod: {_academicYearProvider.LastAcademicYearFundingPeriod}");


            var expiredApprenticeshipUpdates =
                await
                _apprenticeshipUpdateRepository.GetExpiredApprenticeshipUpdates(_academicYearProvider.CurrentAcademicYearStartDate);

            // Logging

            foreach (var update in expiredApprenticeshipUpdates)
            {

            }
        }
    }
}