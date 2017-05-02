using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public class HistoryTransactions : IHistoryTransactions
    {
        private readonly ICommitmentsLogger _logger;

        public HistoryTransactions(ICommitmentsLogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public async Task UpdateApprenticeshipStatus(
            IDbConnection connection,
            IDbTransaction trans,
            PaymentStatus newStatus,
            ApprenticeshipHistoryItem apprenticeshipHistoryItem)
        {
            _logger.Debug($"Creating history item for updating apprenticehsip status to {newStatus}: {apprenticeshipHistoryItem.ApprenticeshipId}",
                    apprenticeshipId: apprenticeshipHistoryItem.ApprenticeshipId);

            await WriteHistory(connection, trans, apprenticeshipHistoryItem, ApprenticeshipChangeType.ChangeOfStatus);
        }

        private async Task WriteHistory(IDbConnection connection, IDbTransaction trans, ApprenticeshipHistoryItem apprenticeshipHistoryItem, ApprenticeshipChangeType changeType)
        {
            if (string.IsNullOrEmpty(apprenticeshipHistoryItem.UserId))
                _logger.Warn($"Missing user id for history item. ChangeType: {changeType}, Role {apprenticeshipHistoryItem.UpdatedByRole}");

            var parameters = new DynamicParameters();
            parameters.Add("@apprenticeshipId", apprenticeshipHistoryItem.ApprenticeshipId, DbType.Int64);
            parameters.Add("@userId", apprenticeshipHistoryItem.UserId, DbType.String);
            parameters.Add("@updatedByRole", apprenticeshipHistoryItem.UpdatedByRole.ToString(), DbType.String);
            parameters.Add("@changeType", changeType.ToString(), DbType.String);
            parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);
            parameters.Add("@updatedByName", apprenticeshipHistoryItem.UpdatedByName, DbType.String);

            await connection.QueryAsync<long>(
                sql:
                "INSERT INTO [dbo].[ApprenticeshipHistory](ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn, UpdatedByName) " +
                "VALUES (@apprenticeshipId, @userId, @updatedByRole, @changeType, @createdOn, @updatedByName); ",
                param: parameters,
                commandType: CommandType.Text,
                transaction: trans);
        }
    }
}
