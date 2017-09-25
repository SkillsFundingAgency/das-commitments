using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.Updater
{
    public class AcademicYearEndExpiryProcessor : IAcademicYearEndExpiryProcessor
    {
        private readonly ILog _logger;
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly ICurrentDateTime _currentDateTime;
        private List<DataLockStatus> _expirableDatalocks = new List<DataLockStatus>();

        [SuppressMessage("ReSharper", "MergeConditionalExpression")] // prevents R# causing C#7 future shock
        public AcademicYearEndExpiryProcessor(ILog logger, IAcademicYearDateProvider academicYearProvider, IDataLockRepository dataLockRepository,  ICurrentDateTime currentDateTime)
        {
            _logger = logger != null ? logger : throw new ArgumentException(nameof(logger));
            _dataLockRepository = dataLockRepository != null ? dataLockRepository : throw new ArgumentException(nameof(dataLockRepository));
            _currentDateTime = currentDateTime != null ? currentDateTime : throw new ArgumentException(nameof(currentDateTime));
            _academicYearProvider = academicYearProvider != null ? academicYearProvider : throw new ArgumentException(nameof(academicYearProvider));
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