using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs
{
    public class ImportFrameworksJob
    {
        private readonly ILogger<ImportStandardsJob> _logger;
        private readonly IApprovalsOuterApiClient _apiClient;
        private readonly IProviderCommitmentsDbContext _providerContext;
        
        public ImportFrameworksJob (ILogger<ImportStandardsJob> logger, IApprovalsOuterApiClient apiClient, IProviderCommitmentsDbContext providerContext)
        {
            _logger = logger;
            _apiClient = apiClient;
            _providerContext = providerContext;
        }

    public async Task Import([TimerTrigger("45 10 1 * * *", RunOnStartup = true)] TimerInfo timer)
    {
        _logger.LogInformation("ImportFrameworksJob - Started");
        var response = await _apiClient.Get<FrameworkResponse>(new GetFrameworksRequest());
        var batches = response.Frameworks.Batch(1000).Select(b => b.ToDataTable(
            p => p.Id, 
            p=> p.FrameworkCode, 
            p=> p.FrameworkName, 
            p=> p.Level, 
            p=> p.PathwayCode, 
            p=> p.PathwayName, 
            p=> p.ProgrammeType, 
            p=> p.Title,
            p=> p.Duration,
            p=> p.MaxFunding,
            p=> p.EffectiveFrom,
            p=> p.EffectiveTo
        ));

        foreach (var batch in batches)
        {
            await ImportFrameworks(_providerContext, batch);
        }


        var fundingPeriodItems = new List<FundingPeriodItem>();

        foreach (var frameworkSummary in response.Frameworks)
        {
            var frameworkId = frameworkSummary.Id;
            fundingPeriodItems.AddRange(frameworkSummary.FundingPeriods.Select(fundingPeriod => new FundingPeriodItem
            {
                FrameworkId = frameworkId, 
                EffectiveFrom = fundingPeriod.EffectiveFrom, 
                EffectiveTo = fundingPeriod.EffectiveTo, 
                FundingCap = fundingPeriod.FundingCap
            }));
        }

        var fundingBatches = fundingPeriodItems.Batch(1000).Select(b =>
            b.ToDataTable(
                p=> p.FrameworkId,
                p=>p.FundingCap,
                p => p.EffectiveFrom,
                p=>p.EffectiveTo
            ));
        foreach (var batch in fundingBatches)
        {
            await ImportFrameworksFunding(_providerContext, batch);
        }
        _logger.LogInformation("ImportFrameworksJob - Finished");
        }

        private static Task ImportFrameworks(IProviderCommitmentsDbContext db, DataTable frameworksDataTable)
        {
            var standards = new SqlParameter("frameworks", SqlDbType.Structured)
            {
                TypeName = "Frameworks",
                Value = frameworksDataTable
            };

            return db.ExecuteSqlCommandAsync("EXEC ImportFrameworks @frameworks", standards);
        }

        private static Task ImportFrameworksFunding(IProviderCommitmentsDbContext db, DataTable standardsFundingDataTable)
        {
            var standardsFunding = new SqlParameter("frameworksFunding", SqlDbType.Structured)
            {
                TypeName = "FrameworksFunding",
                Value = standardsFundingDataTable
            };
            return db.ExecuteSqlCommandAsync("EXEC ImportFrameworksFunding @frameworksFunding", standardsFunding);
        }
    }
}