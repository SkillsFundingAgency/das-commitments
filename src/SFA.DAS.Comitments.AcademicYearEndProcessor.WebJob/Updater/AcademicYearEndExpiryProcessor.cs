using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.Updater
{
    public class AcademicYearEndExpiryProcessor : IAcademicYearEndExpiryProcessor
    {
        private readonly IAcademicYearDateProvider _academicYearProvider;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly DataLockErrorCode _expirableErrorCodes;
        private readonly ICurrentDateTime _currentDateTime;
        private List<DataLockStatus> _expirableDatalocks = new List<DataLockStatus>();

        public AcademicYearEndExpiryProcessor(IAcademicYearDateProvider academicYearProvider,
            IDataLockRepository dataLockRepository, DataLockErrorCode expirableErrorCodes,
            ICurrentDateTime currentDateTime)
        {
            _dataLockRepository = dataLockRepository ?? throw new ArgumentException("dataLockRepository");
            _currentDateTime = currentDateTime ?? throw new ArgumentException("currentDateTime");
            _academicYearProvider = academicYearProvider ?? throw new ArgumentException("academicYearProvider");
            _expirableErrorCodes = expirableErrorCodes;
        }

        public async Task RunUpdate()
        {

            if (_currentDateTime.Now > _academicYearProvider.CurrentAcademicYearEndDate)
            {
                throw new InvalidAcademicYearException($"The academic year dates are not valid for the current time: {_currentDateTime.Now}.");
            }

            if (
                _currentDateTime.Now >= _academicYearProvider.CurrentAcademicYearStartDate
                && _currentDateTime.Now < _academicYearProvider.LastAcademicYearFundingPeriod)
            {
                return;
            }

            _expirableDatalocks =
                await _dataLockRepository.GetExpirableDataLocks(_academicYearProvider.CurrentAcademicYearStartDate,
                    _expirableErrorCodes);

            if (_expirableDatalocks.Any())
            {
                foreach (var expirableDatalock in _expirableDatalocks)
                {
                    await _dataLockRepository.UpdateExpirableDataLocks(expirableDatalock.ApprenticeshipId, expirableDatalock.PriceEpisodeIdentifier);
                }
            }
        }
    }
}