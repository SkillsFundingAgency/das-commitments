using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.CommitmentsV2.Domain.Entities.DataLock;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data.Transactions
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

        public async Task<long> ResolveDataLock(IDbConnection connection, IDbTransaction trans, long dataLockEventId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@DataLockEventId", dataLockEventId);

            return await connection.ExecuteAsync(
                sql: "UPDATE [dbo].[DataLockStatus] " 
                   + "SET IsResolved = 1 " 
                   + "WHERE DataLockEventId = @DataLockEventId",
                param: parameters,
                transaction: trans,
                commandType: CommandType.Text);
        }
    }
}
