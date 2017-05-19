using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public sealed class ProviderPaymentRepository : BaseRepository, IProviderPaymentRepository
    {
        public ProviderPaymentRepository(string databaseConnectionString) : base(databaseConnectionString)
        { }

        public async Task<IList<ProviderPaymentPriorityItem>> GetCustomProviderPaymentPriority(long employerAccountId)
        {
            var results = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployerAccountId", employerAccountId);

                return await c.QueryAsync<ProviderPaymentPriorityItem>(
                    sql: $"[dbo].[GetCustomProviderPaymentPriority]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });

            return results.ToList();
        }

        public Task UpdateProviderPaymentPriority(long employerAccountid, IList<long> newPriorityList)
        {
            return null;
        }
    }
}
