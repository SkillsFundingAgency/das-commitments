using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using System.Text.RegularExpressions;
using System.Globalization;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public sealed class FilterOutAcademicYearRollOverDataLocks(Lazy<ProviderCommitmentsDbContext> db, ILogger<FilterOutAcademicYearRollOverDataLocks> logger)
    : IFilterOutAcademicYearRollOverDataLocks
{
    private static readonly Regex AugustPricePeriodFormat = new(@"08\/\d{4}$", RegexOptions.None, new TimeSpan(0, 0, 0, 1));

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
                    IsAugustPriceEpisode = AugustPricePeriodFormat.IsMatch(x.PriceEpisodeIdentifier)
                })
                .OrderByDescending(x => x.PriceEpisodeIdDateTime).First();

            if (!augustDataLock.IsAugustPriceEpisode)
            {
                var message = $"Unexpected price episode identifier matched: {augustDataLock.PriceEpisodeIdentifier} for apprenticeship: {apprenticeshipId}";
                var exception = new AcademicYearFilterException(message);
                logger.LogError(exception, "An exception occurred whilst filtering out academic year roll over data locks.");
                continue;
            }

            logger.LogInformation("Found an academic year rollover data lock to delete: DataLockEventId: {DataLockEventId}", augustDataLock.DataLockEventId);
            await DeleteDataLock(augustDataLock.DataLockEventId);
        }
    }

    private async Task<List<DataLockStatus>> GetDataLocks(long apprenticeshipId, bool includeRemoved)
    {
        var datalocksQuery = db.Value.DataLocks.Where(x => x.ApprenticeshipId == apprenticeshipId);

        if (includeRemoved)
        {
            datalocksQuery.Where(x => x.EventStatus != Types.EventStatus.Removed && !x.IsExpired);
        }

        datalocksQuery.OrderBy(x => x.IlrEffectiveFromDate).ThenBy(x => x.Id);

        return await datalocksQuery.ToListAsync();
    }

    private static DateTime GetDateFromPriceEpisodeIdentifier(DataLockStatus dataLockStatus)
    {
        return
            DateTime.ParseExact(dataLockStatus.PriceEpisodeIdentifier.Substring(dataLockStatus.PriceEpisodeIdentifier.Length - 10), "dd/MM/yyyy",
                new CultureInfo("en-GB"));
    }

    private async Task DeleteDataLock(long dataLockEventId)
    {
        var datalock = await db.Value.DataLocks.Where(x => x.DataLockEventId == dataLockEventId).FirstOrDefaultAsync();
        db.Value.DataLocks.Remove(datalock);
        await db.Value.SaveChangesAsync();
    }
}