using MoreLinq.Extensions;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class ImportStandardsJob(ILogger<ImportStandardsJob> logger, IApprovalsOuterApiClient apiClient, IProviderCommitmentsDbContext providerContext)
{
    public async Task Import([TimerTrigger("45 10 1 * * *", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("ImportStandardsJob - Started");

        var response = await apiClient.Get<StandardResponse>(new GetStandardsRequest());

        var filteredStandards = FilterResponse(response);

        await ProcessStandards(filteredStandards);

        await ProcessStandardOptions(filteredStandards);

        await ProcessFunding(filteredStandards);

        logger.LogInformation("ImportStandardsJob - Finished");
    }

    private async Task ProcessFunding(IEnumerable<StandardSummary> filteredStandards)
    {
        var fundingPeriodItems = new List<FundingPeriodItem>();

        var uniqueLarsCodes = filteredStandards.GroupBy(s => s.LarsCode).
            Select(t => t.OrderByDescending(x => x.VersionMajor).ThenByDescending(y => y.VersionMinor).FirstOrDefault());

        foreach (var responseStandard in uniqueLarsCodes)
        {
            var larsCode = responseStandard.LarsCode;
            fundingPeriodItems.AddRange(responseStandard.FundingPeriods.Select(fundingPeriod => new FundingPeriodItem
            {
                StandardId = larsCode,
                EffectiveFrom = fundingPeriod.EffectiveFrom,
                EffectiveTo = fundingPeriod.EffectiveTo,
                FundingCap = fundingPeriod.FundingCap
            }));
        }

        var fundingBatches = fundingPeriodItems.Batch(1000).Select(b =>
            b.ToDataTable(
                p => p.StandardId,
                p => p.FundingCap,
                p => p.EffectiveFrom,
                p => p.EffectiveTo
            ));
        foreach (var batch in fundingBatches)
        {
            await ImportStandardsFunding(providerContext, batch);
        }
    }

    private async Task ProcessStandardOptions(IEnumerable<StandardSummary> filteredStandards)
    {
        var options = filteredStandards.SelectMany(s => s.Options, (s, o) => new { StandardUId = s.StandardUId.Trim(), Option = o.Trim() });

        var optionBatches = options.Batch(1000).Select(b => b.ToDataTable(
            p => p.StandardUId,
            p => p.Option));

        await ClearStandardOptions(providerContext);
        
        foreach (var batch in optionBatches)
        {
            await ImportStandardOptions(providerContext, batch);
        }
    }

    private async Task ProcessStandards(IEnumerable<StandardSummary> filteredStandards)
    {
        var batches = filteredStandards.Batch(1000).Select(b => b.ToDataTable(
            p => p.StandardUId,
            p => p.LarsCode,
            p => p.IFateReferenceNumber,
            p => p.Version,
            p => p.Title,
            p => p.Level,
            p => p.Duration,
            p => p.CurrentFundingCap,
            p => p.EffectiveFrom,
            p => p.LastDateForNewStarts,
            p => p.VersionMajor,
            p => p.VersionMinor,
            p => p.StandardPageUrl,
            p => p.Status,
            p => p.IsLatestVersion,
            p => p.VersionEarliestStartDate,
            p => p.VersionLatestStartDate,
            p => p.Route
        ));

        foreach (var batch in batches)
        {
            await ImportStandards(providerContext, batch);
        }
    }
    private static List<StandardSummary> FilterResponse(StandardResponse response)
    {
        var statusList = new[] { "Approved for delivery", "Retired" };
        var filteredStandards = response.Standards.Where(s => statusList.Contains(s.Status)).ToList();

        var latestVersionsOfStandards = filteredStandards.
            GroupBy(s => s.LarsCode).
            Select(c => c.OrderByDescending(x => x.VersionMajor).ThenByDescending(y => y.VersionMinor).FirstOrDefault());

        var latestVersionsStandardUIds = latestVersionsOfStandards.Select(s => s.StandardUId);

        foreach (var latestStandard in filteredStandards.Where(s => latestVersionsStandardUIds.Contains(s.StandardUId)))
        {
            latestStandard.IsLatestVersion = true;
        }

        return filteredStandards;
    }

    private static Task ImportStandards(IProviderCommitmentsDbContext db, DataTable standardsDataTable)
    {
        var standards = new SqlParameter("standards", SqlDbType.Structured)
        {
            TypeName = "Standards",
            Value = standardsDataTable
        };

        return db.ExecuteSqlCommandAsync("EXEC ImportStandards @standards", standards);
    }

    private static Task ImportStandardOptions(IProviderCommitmentsDbContext db, DataTable standardOptionsDataTable)
    {
        var standardOptions = new SqlParameter("standardOptions", SqlDbType.Structured)
        {
            TypeName = "StandardOptions",
            Value = standardOptionsDataTable
        };

        return db.ExecuteSqlCommandAsync("EXEC ImportStandardOptions @standardOptions", standardOptions);
    }

    private static Task ClearStandardOptions(IProviderCommitmentsDbContext db)
    {
        return db.ExecuteSqlCommandAsync("EXEC TruncateStandardOptions");
    }

    private static Task ImportStandardsFunding(IProviderCommitmentsDbContext db, DataTable standardsFundingDataTable)
    {
        var standardsFunding = new SqlParameter("standardsFunding", SqlDbType.Structured)
        {
            TypeName = "StandardsFunding",
            Value = standardsFundingDataTable
        };
        
        return db.ExecuteSqlCommandAsync("EXEC ImportStandardsFunding @standardsFunding", standardsFunding);
    }
}