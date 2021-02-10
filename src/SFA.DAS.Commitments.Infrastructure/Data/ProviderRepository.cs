using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class ProviderRepository : BaseRepository, IProviderRepository
    {
        public ProviderRepository(string connectionString, ILog logger) : base(connectionString, logger)
        {
        }

        public async Task<List<Domain.Entities.Provider>> GetProviders()
        {
            return await WithConnection(async c =>
            {
                var results = await c.QueryAsync<Domain.Entities.Provider>(
                    $@"SELECT Ukprn, Name
                            FROM [dbo].[Providers]");
                return results.AsList();
            });
        }

        public async Task<Domain.Entities.Provider> GetProvider(long ukPrn)
        {
            return await WithConnection(async c =>
            {
                var results = await c.QuerySingleAsync<Domain.Entities.Provider>(
                    $@"SELECT Ukprn, Name
                            FROM [dbo].[Providers]
                            WHERE Ukprn = @id;",
                    param: new {@id = ukPrn});

                return results;
            });
        }
    }
}