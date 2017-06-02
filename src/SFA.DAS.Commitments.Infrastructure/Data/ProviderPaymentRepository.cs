using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public sealed class ProviderPaymentRepository : BaseRepository, IProviderPaymentRepository
    {
        public ProviderPaymentRepository(string databaseConnectionString, ICommitmentsLogger logger) : base(databaseConnectionString, logger)
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

        public async Task UpdateProviderPaymentPriority(long employerAccountId, IList<ProviderPaymentPriorityUpdateItem> newPriorityList)
        {
            var providerIdTable = new DataTable();

            providerIdTable.Columns.Add("Id", typeof(long));
            providerIdTable.Columns.Add("Priority", typeof(int));

            foreach(var item in newPriorityList)
            { 
                providerIdTable.Rows.Add(item.ProviderId, item.PriorityOrder);
            }

            await WithTransaction(async (c, t) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployerAccountId", employerAccountId);
                parameters.Add("@ProviderIds", providerIdTable);

                await c.ExecuteAsync(
                    sql: "UpdateCustomProviderPaymentPriority",
                    transaction: t,
                    param: new { @EmployerAccountId = employerAccountId, @ProviderIds = providerIdTable.AsTableValuedParameter("dbo.ProviderPriorityTable") },
                    commandType: CommandType.StoredProcedure);
            });
        }
    }
}
