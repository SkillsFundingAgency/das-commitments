using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class HistoryRepository : BaseRepository, IHistoryRepository
    {
        public HistoryRepository(string connectionString, ICommitmentsLogger logger) : base(connectionString, logger)
        {
        }

        public async Task InsertHistory(IEnumerable<HistoryItem> historyItems)
        {
            await WithTransaction(async (connection, transaction) =>
            {
                foreach (var historyItem in historyItems)
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

                    await connection.ExecuteAsync(
                        sql: "InsertHistory",
                        param: parameters,
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure);
                }
            });
        }
    }
}
