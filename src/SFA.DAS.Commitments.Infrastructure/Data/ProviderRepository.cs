using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public sealed class ProviderRepository : BaseRepository, IProviderRepository
    {
        public ProviderRepository(string databaseConnectionString) : base(databaseConnectionString)
        { }

        public async Task<IList<ProviderPaymentPriorityItem>> GetCustomProviderPaymentPriority(long employerAccountId)
        {
            var results = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", employerAccountId);

                return await c.QueryAsync<ProviderPaymentPriorityItem>(
                    sql: $"SELECT * FROM [dbo].[CustomProviderPaymentPriority] WHERE EmployerAccountId = @id;",
                    param: parameters,
                    commandType: CommandType.Text);
            });

            return results.ToList();
        }
    }
}
