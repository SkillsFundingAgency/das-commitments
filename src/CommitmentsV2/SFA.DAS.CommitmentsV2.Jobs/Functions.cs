using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.Providers.Api.Client;

namespace SFA.DAS.CommitmentsV2.Jobs
{
    public class Functions
    {
        private readonly ILogger<Functions> _logger;
        private readonly IProviderApiClient _providerApiClient;
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public Functions(ILogger<Functions> logger, IProviderApiClient providerApiClient, Lazy<ProviderCommitmentsDbContext> db)
        {
            _logger = logger;
            _providerApiClient = providerApiClient;
            _db = db;
        }

        public async Task ImportProvidersJob([TimerTrigger("45 10 1 * * *", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation("ImportProvidersJob - Started");
            var providers = await _providerApiClient.FindAllAsync();
            var batches = providers.Batch(1000).Select(b => b.ToDataTable(p => p.Ukprn, p => p.ProviderName));

            foreach (var batch in batches)
            {
                await ImportProviders(_db.Value, batch);
            }
            _logger.LogInformation("ImportProvidersJob - Finished");
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
}
