using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class HistoryRepository : BaseRepository, IHistoryRepository
    {
        public HistoryRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task InsertHistory(HistoryItem historyItem)
        {
            await WithConnection(async (connection) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@entityType", historyItem.EntityType, DbType.String);
                parameters.Add("@entityId", historyItem.EntityId, DbType.Int64);
                parameters.Add("@userId", historyItem.UserId, DbType.String);
                parameters.Add("@updatedByRole", historyItem.UpdatedByRole, DbType.String);
                parameters.Add("@changeType", historyItem.ChangeType, DbType.String);
                parameters.Add("@updatedByName", historyItem.UpdatedByName, DbType.String);
                parameters.Add("@originalState", historyItem.OriginalState, DbType.String);
                parameters.Add("@updatedState", historyItem.UpdatedState, DbType.String);

                return await connection.ExecuteAsync(
                    sql: "InsertHistory",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });
        }
    }
}
