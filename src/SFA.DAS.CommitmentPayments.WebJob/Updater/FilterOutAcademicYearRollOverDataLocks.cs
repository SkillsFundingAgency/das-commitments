using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using System.Text.RegularExpressions;
using SFA.DAS.NLog.Logger;
using System;
using System.Globalization;

namespace SFA.DAS.CommitmentPayments.WebJob.Updater
{
    public sealed class FilterOutAcademicYearRollOverDataLocks : IFilterOutAcademicYearRollOverDataLocks
    {
        private IDataLockRepository _dataLockRepository;
        private static Regex _augustPricePeriodFormat = new Regex(@"08\/\d{4}$");
        private ILog _logger;

        public FilterOutAcademicYearRollOverDataLocks(IDataLockRepository dataLockRepository, ILog logger)
        {
            _dataLockRepository = dataLockRepository;
            _logger = logger;
        }

        public async Task Filter(long apprenticeshipId)
        {
            var apprenticeshipDataLocks = await _dataLockRepository.GetDataLocks(apprenticeshipId);

            if (apprenticeshipDataLocks == null || apprenticeshipDataLocks.Count == 0)
                return;

            var haveDuplicates = apprenticeshipDataLocks
                .GroupBy(x => new
                    {
                        x.IlrTrainingCourseCode,
                        x.IlrTrainingType,
                        x.IlrActualStartDate,
                        x.IlrEffectiveFromDate,
                        x.IlrTotalCost
                    })
                .Where(g => g.Count() > 1);
            
            foreach(var group in haveDuplicates)
            {
                var augustDataLock = group
                    .Select(x => new
                        {
                            DataLockEventId = x.DataLockEventId,
                            PriceEpisodeIdentifier = x.PriceEpisodeIdentifier,
                            PriceEpisodeIdDateTime = DateTime.ParseExact(x.PriceEpisodeIdentifier.Substring(x.PriceEpisodeIdentifier.Length - 10), "dd/MM/yyyy", new CultureInfo("en-GB")),
                            IsAugustPriceEpisode = _augustPricePeriodFormat.IsMatch(x.PriceEpisodeIdentifier)
                    })
                    .OrderByDescending(x => x.PriceEpisodeIdDateTime).First();

                if (!augustDataLock.IsAugustPriceEpisode)
                {
                    var message = $"Unexpected price episode identifier matched: {augustDataLock.PriceEpisodeIdentifier} for apprenticeship: {apprenticeshipId}";
                    var exception = new AcademicYearFilterException(message);
                    _logger.Error(exception, message);
                    continue;
                }

                _logger.Info($"Found an academic year rollover data lock to delete: DataLockEventId: {augustDataLock.DataLockEventId}");
                await _dataLockRepository.Delete(augustDataLock.DataLockEventId);
            }
        }
    }
}
