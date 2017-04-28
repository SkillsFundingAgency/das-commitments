using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public class DataLockTransactions : IDataLockTransactions
    {
        public async Task<long> UpdateDataLockTriageStatus(IDbConnection connection,
            IDbTransaction trans, long dataLockEventId, TriageStatus triageStatus)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@DataLockEventId", dataLockEventId);
            parameters.Add("@TriageStatus", triageStatus);
            return await connection.ExecuteAsync(
                sql: $"[dbo].[UpdateDataLockTriageStatus]",
                param: parameters,
                transaction: trans,
                commandType: CommandType.StoredProcedure);
        }
    }
}
