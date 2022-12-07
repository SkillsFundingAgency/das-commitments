using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public sealed class FilterOutAcademicYearRollOverDataLocks : IFilterOutAcademicYearRollOverDataLocks
    {
        private static Regex _augustPricePeriodFormat = new Regex(@"08\/\d{4}$");
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly ILogger<FilterOutAcademicYearRollOverDataLocks> _logger;

        public FilterOutAcademicYearRollOverDataLocks(Lazy<ProviderCommitmentsDbContext> db, ILogger<FilterOutAcademicYearRollOverDataLocks> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Filter(long apprenticeshipId)
        {
            var apprenticeshipDataLocks = await GetDataLocks(apprenticeshipId, true);

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

            foreach (var group in haveDuplicates)
            {
                var augustDataLock = group
                    .Select(x => new
                    {
                        DataLockEventId = x.DataLockEventId,
                        PriceEpisodeIdentifier = x.PriceEpisodeIdentifier,
                        PriceEpisodeIdDateTime = GetDateFromPriceEpisodeIdentifier(x),
                        IsAugustPriceEpisode = _augustPricePeriodFormat.IsMatch(x.PriceEpisodeIdentifier)
                    })
                    .OrderByDescending(x => x.PriceEpisodeIdDateTime).First();

                if (!augustDataLock.IsAugustPriceEpisode)
                {
                    var message = $"Unexpected price episode identifier matched: {augustDataLock.PriceEpisodeIdentifier} for apprenticeship: {apprenticeshipId}";
                    var exception = new AcademicYearFilterException(message);
                    _logger.LogError(exception, message);
                    continue;
                }

                _logger.LogInformation($"Found an academic year rollover data lock to delete: DataLockEventId: {augustDataLock.DataLockEventId}");
                await DeleteDataLock(augustDataLock.DataLockEventId);
            }
        }

        private async Task<List<DataLockStatus>> GetDataLocks(long apprenticeshipId, bool includeRemoved)
        {
            var datalocksQuery = _db.Value.DataLocks.Where(x => x.ApprenticeshipId == apprenticeshipId);

            if (includeRemoved)
            {
                datalocksQuery.Where(x => x.EventStatus != Types.EventStatus.Removed && !x.IsExpired);
            }

            datalocksQuery.OrderBy(x => x.IlrEffectiveFromDate).ThenBy(x => x.Id);

            var datalocks = await datalocksQuery.ToListAsync();

            return datalocks;
        }

        private DateTime GetDateFromPriceEpisodeIdentifier(DataLockStatus dataLockStatus)
        {
            return
            DateTime.ParseExact(dataLockStatus.PriceEpisodeIdentifier.Substring(dataLockStatus.PriceEpisodeIdentifier.Length - 10), "dd/MM/yyyy",
                new CultureInfo("en-GB"));
        }

        private async Task DeleteDataLock(long dataLockEventId)
        {
            var datalock = await _db.Value.DataLocks.Where(x => x.DataLockEventId == dataLockEventId).FirstOrDefaultAsync();
            _db.Value.DataLocks.Remove(datalock);
            await _db.Value.SaveChangesAsync();
        }
    }
}