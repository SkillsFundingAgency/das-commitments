using MoreLinq.Extensions;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class ImportProvidersJobs(ILogger<ImportProvidersJobs> logger, IApprovalsOuterApiClient apiClient, Lazy<ProviderCommitmentsDbContext> db)
{
    public async Task ImportProvidersJob([TimerTrigger("45 10 1 * * *", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("ImportProvidersJob - Started");

        var response = await apiClient.Get<ProviderResponse>(new GetProvidersRequest());
        var batches = response.Providers.Batch(1000).Select(b => b.ToDataTable(p => p.Ukprn, p => p.Name));

        foreach (var batch in batches)
        {
            await ImportProviders(db.Value, batch);
        }

        logger.LogInformation("ImportProvidersJob - Finished");
    }

    private static Task ImportProviders(ProviderCommitmentsDbContext db, DataTable providersDataTable)
    {
        var providers = new SqlParameter("providers", SqlDbType.Structured)
        {
            TypeName = "Providers",
            Value = providersDataTable
        };

        var now = new SqlParameter("now", DateTime.UtcNow);

        return db.ExecuteSqlCommandAsync("EXEC ImportProviders @providers, @now", providers, now);
    }
}