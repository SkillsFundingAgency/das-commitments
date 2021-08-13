using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.Api;
using SFA.DAS.CommitmentsV2.Models.Api.Types;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class ImportStandardsJob
    {
        private readonly ILogger<ImportStandardsJob> _logger;
        private readonly IApiClient _apiClient;
        private readonly IProviderCommitmentsDbContext _providerContext;

        public ImportStandardsJob(ILogger<ImportStandardsJob> logger, IApiClient apiClient, IProviderCommitmentsDbContext providerContext)
        {
            _logger = logger;
            _apiClient = apiClient;
            _providerContext = providerContext;
        }

        public async Task Import([TimerTrigger("45 10 1 * * *", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation("ImportStandardsJob - Started");

            var response = await _apiClient.Get<StandardResponse>(new GetStandardsRequest());

            var filteredStandards = FilterResponse(response);

            await ProcessStandards(filteredStandards);

            await ProcessStandardOptions(filteredStandards);

            await ProcessFunding(filteredStandards);

            _logger.LogInformation("ImportStandardsJob - Finished");
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
                await ImportStandardsFunding(_providerContext, batch);
            }
        }

        private async Task ProcessStandardOptions(IEnumerable<StandardSummary> filteredStandards)
        {
            var options = filteredStandards.SelectMany(s => s.Options, (s, o) => new { StandardUId = s.StandardUId.Trim(), Option = o.Trim() });

            var optionBatches = options.Batch(1000).Select(b => b.ToDataTable(
                p => p.StandardUId,
                p => p.Option));

            foreach (var batch in optionBatches)
            {
                await ImportStandardOptions(_providerContext, batch);
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
                p => p.VersionEarliestStartDate,
                p => p.VersionLatestStartDate,
                p => p.VersionMajor,
                p => p.VersionMinor,
                p => p.StandardPageUrl,
                p => p.Status,
                p => p.IsLatestVersion
                ));

            foreach (var batch in batches)
            {
                await ImportStandards(_providerContext, batch);
            }
        }
        private IEnumerable<StandardSummary> FilterResponse(StandardResponse response)
        {
            var statusList = new string[] { "Approved for delivery", "Retired" };
            var filteredStandards = response.Standards.Where(s => statusList.Contains(s.Status));

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
}