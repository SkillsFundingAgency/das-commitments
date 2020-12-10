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
        private readonly ICommitmentsDbContext _providerContext;

        public ImportStandardsJob(ILogger<ImportStandardsJob> logger, IApiClient apiClient, ICommitmentsDbContext providerContext)
        {
            _logger = logger;
            _apiClient = apiClient;
            _providerContext = providerContext;
        }

        public async Task Import([TimerTrigger("45 10 1 * * *", RunOnStartup = true)] TimerInfo timer)
        { 
            _logger.LogInformation("ImportStandardsJob - Started");

            var response = await _apiClient.Get<StandardResponse>(new GetStandardsRequest());
            var batches = response.Standards.Batch(1000).Select(b => b.ToDataTable(
                p => p.Id, 
                p => p.Title,
                p=>p.Level,
                p=>p.Duration,
                p=>p.CurrentFundingCap,
                p=>p.EffectiveFrom,
                p=>p.LastDateForNewStarts
                ));

            foreach (var batch in batches)
            {
                await ImportStandards(_providerContext, batch);
            }


            var fundingPeriodItems = new List<FundingPeriodItem>();

            foreach (var responseStandard in response.Standards)
            {
                var standardId = responseStandard.Id;
                fundingPeriodItems.AddRange(responseStandard.FundingPeriods.Select(fundingPeriod => new FundingPeriodItem
                {
                    StandardId = standardId, 
                    EffectiveFrom = fundingPeriod.EffectiveFrom, 
                    EffectiveTo = fundingPeriod.EffectiveTo, 
                    FundingCap = fundingPeriod.FundingCap
                }));
            }
            
            var fundingBatches = fundingPeriodItems.Batch(1000).Select(b =>
                b.ToDataTable(
                    p=> p.StandardId,
                    p=>p.FundingCap,
                    p => p.EffectiveFrom,
                    p=>p.EffectiveTo
                ));
            foreach (var batch in fundingBatches)
            {
                await ImportStandardsFunding(_providerContext, batch);
            }

            _logger.LogInformation("ImportStandardsJob - Finished");
        }

        private static Task ImportStandards(ICommitmentsDbContext db, DataTable standardsDataTable)
        {
            var standards = new SqlParameter("standards", SqlDbType.Structured)
            {
                TypeName = "Standards",
                Value = standardsDataTable
            };

            return db.ExecuteSqlCommandAsync("EXEC ImportStandards @standards", standards);
        }

        private static Task ImportStandardsFunding(ICommitmentsDbContext db, DataTable standardsFundingDataTable)
        {
            var standardsFunding = new SqlParameter("StandardsFunding", SqlDbType.Structured)
            {
                TypeName = "StandardsFunding",
                Value = standardsFundingDataTable
            };
            return db.ExecuteSqlCommandAsync("EXEC ImportStandardsFunding @standardsFunding", standardsFunding);
        }
    }
}