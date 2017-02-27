using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
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
            _logger.Debug($"Creating history item for commitment: {commitmentHistoryItem.CommitmentId}", 
                commitmentId: commitmentHistoryItem.CommitmentId);

            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", commitmentHistoryItem.CommitmentId, DbType.Int64);
            parameters.Add("@userId", commitmentHistoryItem.UserId, DbType.Int64);
            parameters.Add("@updatedByRole", commitmentHistoryItem.UpdatedByRole, DbType.Int16);
            parameters.Add("@changeType", CommitmentChangeType.Create, DbType.Int16);
            parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);

            await connection.QueryAsync<long>(
                sql:
                "INSERT INTO [dbo].[CommitmentHistory](CommitmentId, UserId, UpdatedByRole, ChangeType, CreatedOn) " +
                "VALUES (@commitmentId, @userId, @updatedByRole, @changeType, @createdOn); ",
                param: parameters,
                commandType: CommandType.Text,
                transaction: trans);
        }


        // Apprenticeship

        public async Task CreateApprenticeship(
            IDbConnection connection,
            IDbTransaction trans,
            ApprenticeshipHistoryItem apprenticeshipHistoryItem)
        {
            
            _logger.Debug($"Creating history item for apprenticehsip: {apprenticeshipHistoryItem.ApprenticeshipId}",
                    apprenticeshipId: apprenticeshipHistoryItem.ApprenticeshipId);
            
            var parameters = new DynamicParameters();
            parameters.Add("@apprenticeshipId", apprenticeshipHistoryItem.ApprenticeshipId, DbType.Int64);
            parameters.Add("@userId", apprenticeshipHistoryItem.UserId, DbType.Int64);
            parameters.Add("@updatedByRole", apprenticeshipHistoryItem.UpdatedByRole, DbType.Int16);
            parameters.Add("@changeType", ApprenticeshipChangeType.Created, DbType.Int16);
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

    public interface IHistoryTransactions
    {
        Task CreateCommitment(IDbConnection connection, IDbTransaction trans, CommitmentHistoryItem commitmentHistoryItem);

        Task CreateApprenticeship(IDbConnection connection, IDbTransaction trans, ApprenticeshipHistoryItem apprenticeshipHistoryItem);
    }
}
