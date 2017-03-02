using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

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


        public async Task CreateCommitment(
            IDbConnection connection,
            IDbTransaction trans,
            CommitmentHistoryItem commitmentHistoryItem)
        {
            _logger.Debug($"History item for creating commitment: {commitmentHistoryItem.CommitmentId}", 
                commitmentId: commitmentHistoryItem.CommitmentId);
            await WriteCommitmentHistory(connection, trans, commitmentHistoryItem, CommitmentChangeType.Created);
        }

        public async Task DeleteCommitment(IDbConnection connection, IDbTransaction trans, CommitmentHistoryItem commitmentHistoryItem)
        {
            _logger.Debug($"History item for deleting commitment: {commitmentHistoryItem.CommitmentId}",
                commitmentId: commitmentHistoryItem.CommitmentId);

            await WriteCommitmentHistory(connection, trans, commitmentHistoryItem, CommitmentChangeType.Deleted);
        }

        public async Task UpdateCommitment(IDbConnection connection, IDbTransaction trans, CommitmentChangeType changeType, CommitmentHistoryItem commitmentHistoryItem)
        {
            _logger.Debug($"History item for updating commitment: {commitmentHistoryItem.CommitmentId}",
               commitmentId: commitmentHistoryItem.CommitmentId);

            await WriteCommitmentHistory(connection, trans, commitmentHistoryItem, changeType);
        }

        public async Task AddApprenticeshipForCommitment(IDbConnection connection, IDbTransaction trans, CommitmentHistoryItem commitmentHistoryItem)
        {
            _logger.Debug($"History item for adding apprenticeship to commitment: {commitmentHistoryItem.CommitmentId}",
               commitmentId: commitmentHistoryItem.CommitmentId);

            await WriteCommitmentHistory(connection, trans, commitmentHistoryItem, CommitmentChangeType.CreatedApprenticeship);
        }

        public async Task DeleteApprenticeshipForCommitment(
            IDbConnection connection,
            IDbTransaction transactions,
            CommitmentHistoryItem commitmentHistoryItem)
        {
            _logger.Debug($"History item for deleteing apprenticeship on commitment: {commitmentHistoryItem.CommitmentId}",
               commitmentId: commitmentHistoryItem.CommitmentId);

            await WriteCommitmentHistory(connection, transactions, commitmentHistoryItem, CommitmentChangeType.DeletedApprenticeship);
        }

        public async Task UpdateApprenticeshipForCommitment(
            IDbConnection connection,
            IDbTransaction trans,
            CommitmentHistoryItem commitmentHistoryItem)
        {
            _logger.Debug($"History item for deleteing apprenticeship on commitment: {commitmentHistoryItem.CommitmentId}",
               commitmentId: commitmentHistoryItem.CommitmentId);

            await WriteCommitmentHistory(connection, trans, commitmentHistoryItem, CommitmentChangeType.EditedApprenticeship);
        }

        // Apprenticeship

        public async Task CreateApprenticeship(
            IDbConnection connection,
            IDbTransaction trans,
            ApprenticeshipHistoryItem apprenticeshipHistoryItem)
        {   
            _logger.Debug($"Creating history item for apprenticehsip: {apprenticeshipHistoryItem.ApprenticeshipId}",
                    apprenticeshipId: apprenticeshipHistoryItem.ApprenticeshipId);

            await WriteHistory(connection, trans, apprenticeshipHistoryItem, ApprenticeshipChangeType.Created);
        }

        public async Task UpdateApprenticeship(
            IDbConnection connection,
            IDbTransaction trans,
            ApprenticeshipHistoryItem apprenticeshipHistoryItem)
        {
            _logger.Debug($"Creating history item for updating apprenticehsip: {apprenticeshipHistoryItem.ApprenticeshipId}",
                    apprenticeshipId: apprenticeshipHistoryItem.ApprenticeshipId);

            await WriteHistory(connection, trans, apprenticeshipHistoryItem, ApprenticeshipChangeType.Updated);
        }

        private async Task WriteCommitmentHistory(IDbConnection connection, IDbTransaction trans, CommitmentHistoryItem commitmentHistoryItem, CommitmentChangeType changeType)
        {
            if(string.IsNullOrEmpty(commitmentHistoryItem.UserId))
                _logger.Warn($"Missing user id for history item. ChangeType: {changeType}, Role {commitmentHistoryItem.UpdatedByRole}");

            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", commitmentHistoryItem.CommitmentId, DbType.Int64);
            parameters.Add("@userId", commitmentHistoryItem.UserId, DbType.String);
            parameters.Add("@updatedByRole", commitmentHistoryItem.UpdatedByRole, DbType.Int16);
            parameters.Add("@changeType", changeType, DbType.Int16);
            parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);

            await connection.QueryAsync<long>(
                sql:
                "INSERT INTO [dbo].[CommitmentHistory](CommitmentId, UserId, UpdatedByRole, ChangeType, CreatedOn) " +
                "VALUES (@commitmentId, @userId, @updatedByRole, @changeType, @createdOn); ",
                param: parameters,
                commandType: CommandType.Text,
                transaction: trans);

        }

        private async Task WriteHistory(IDbConnection connection, IDbTransaction trans, ApprenticeshipHistoryItem apprenticeshipHistoryItem, ApprenticeshipChangeType changeType)
        {
            if (string.IsNullOrEmpty(apprenticeshipHistoryItem.UserId))
                _logger.Warn($"Missing user id for history item. ChangeType: {changeType}, Role {apprenticeshipHistoryItem.UpdatedByRole}");

            var parameters = new DynamicParameters();
            parameters.Add("@apprenticeshipId", apprenticeshipHistoryItem.ApprenticeshipId, DbType.Int64);
            parameters.Add("@userId", apprenticeshipHistoryItem.UserId, DbType.String);
            parameters.Add("@updatedByRole", apprenticeshipHistoryItem.UpdatedByRole, DbType.Int16);
            parameters.Add("@changeType", changeType, DbType.Int16);
            parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);

            await connection.QueryAsync<long>(
                sql:
                "INSERT INTO [dbo].[ApprenticeshipHistory](ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn) " +
                "VALUES (@apprenticeshipId, @userId, @updatedByRole, @changeType, @createdOn); ",
                param: parameters,
                commandType: CommandType.Text,
                transaction: trans);
        }
    }
}
