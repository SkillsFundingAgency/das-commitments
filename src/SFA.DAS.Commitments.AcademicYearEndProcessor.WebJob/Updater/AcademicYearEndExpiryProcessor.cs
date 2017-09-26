using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater
{
    public class AcademicYearEndExpiryProcessor : IAcademicYearEndExpiryProcessor
    {
        private readonly ILog _logger;
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly ICurrentDateTime _currentDateTime;

        public AcademicYearEndExpiryProcessor(ILog logger, IAcademicYearDateProvider academicYearProvider, IDataLockRepository dataLockRepository, ICurrentDateTime currentDateTime)
        {

            if (logger == null) throw new ArgumentException(nameof(logger));
            if (dataLockRepository == null) throw new ArgumentException(nameof(dataLockRepository));
            if (currentDateTime == null) throw new ArgumentException(nameof(currentDateTime));
            if (academicYearProvider == null) throw new ArgumentException(nameof(academicYearProvider));


            _logger = logger;
            _dataLockRepository = dataLockRepository;
            _currentDateTime = currentDateTime;
            _academicYearProvider = academicYearProvider;
        }

        public async Task RunUpdate()
        {
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
    }
}