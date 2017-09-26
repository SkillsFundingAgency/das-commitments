using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<DataLockStatus> _expirableDatalocks = new List<DataLockStatus>();

        public AcademicYearEndExpiryProcessor(ILog logger, IAcademicYearDateProvider academicYearProvider, IDataLockRepository dataLockRepository,  ICurrentDateTime currentDateTime)
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

            _expirableDatalocks =
                await _dataLockRepository.GetExpirableDataLocks(_academicYearProvider.CurrentAcademicYearStartDate);

            if (_expirableDatalocks.Any())
            {
                foreach (var expirableDatalock in _expirableDatalocks)
                {
                    await _dataLockRepository.UpdateExpirableDataLocks(expirableDatalock.ApprenticeshipId, expirableDatalock.PriceEpisodeIdentifier, _currentDateTime.Now);
                }
            }

            _logger.Info($"{nameof(AcademicYearEndExpiryProcessor)} expired {_expirableDatalocks.Count} items");

        }
    }
}