using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data.Transactions;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class CommitmentRepository : BaseRepository, ICommitmentRepository
    {
        private readonly ICommitmentsLogger _logger;

        private readonly IHistoryTransactions _historyTransactions;

        private readonly IApprenticeshipTransactions _apprenticeshipTransactions;

        public CommitmentRepository(
            string databaseConnectionString, 
            ICommitmentsLogger logger,
            IHistoryTransactions historyTransactions,
            IApprenticeshipTransactions apprenticeshipTransactions) : base(databaseConnectionString)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (historyTransactions == null)
                throw new ArgumentNullException(nameof(historyTransactions));
            if (historyTransactions == null)
                throw new ArgumentNullException(nameof(apprenticeshipTransactions));

            _logger = logger;
            _historyTransactions = historyTransactions;
            _apprenticeshipTransactions = apprenticeshipTransactions;
        }

        public async Task<long> Create(Commitment commitment, CallerType callerType, string userId)
        {
            _logger.Debug($"Creating commitment with ref: {commitment.Reference}", accountId: commitment.EmployerAccountId, providerId: commitment.ProviderId);

            return await WithConnection(async connection =>
            {
                long commitmentId;

                var parameters = new DynamicParameters();
                parameters.Add("@reference", commitment.Reference, DbType.String);
                parameters.Add("@legalEntityId", commitment.LegalEntityId, DbType.String);
                parameters.Add("@legalEntityName", commitment.LegalEntityName, DbType.String);
                parameters.Add("@LegalEntityAddress", commitment.LegalEntityAddress, DbType.String);
                parameters.Add("@legalEntityOrganisationType", commitment.LegalEntityOrganisationType, DbType.Int16);
                parameters.Add("@accountId", commitment.EmployerAccountId, DbType.Int64);
                parameters.Add("@providerId", commitment.ProviderId, DbType.Int64);
                parameters.Add("@providerName", commitment.ProviderName, DbType.String);
                parameters.Add("@commitmentStatus", commitment.CommitmentStatus, DbType.Int16);
                parameters.Add("@editStatus", commitment.EditStatus, DbType.Int16);
                parameters.Add("@id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);
                parameters.Add("@lastAction", commitment.LastAction, DbType.Int16);
                parameters.Add("@lastUpdateByEmployerName", commitment.LastUpdatedByEmployerName, DbType.String);
                parameters.Add("@lastUpdateByEmployerEmail", commitment.LastUpdatedByEmployerEmail, DbType.String);

                using (var trans = connection.BeginTransaction())
                {
                    commitmentId = (await connection.QueryAsync<long>(
                        sql:
                        "INSERT INTO [dbo].[Commitment](Reference, LegalEntityId, LegalEntityName, LegalEntityAddress, LegalEntityOrganisationType, EmployerAccountId, ProviderId, ProviderName, CommitmentStatus, EditStatus, CreatedOn, LastAction, LastUpdatedByEmployerName, LastUpdatedByEmployerEmail) " +
                        "VALUES (@reference, @legalEntityId, @legalEntityName, @legalEntityAddress, @legalEntityOrganisationType, @accountId, @providerId, @providerName, @commitmentStatus, @editStatus, @createdOn, @lastAction, @lastUpdateByEmployerName, @lastUpdateByEmployerEmail); " +
                        "SELECT CAST(SCOPE_IDENTITY() as int);",
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: trans)).Single();

                    foreach (var apprenticeship in commitment.Apprenticeships)
                    {
                        _logger.Debug($"Creating apprenticeship in new commitment - {apprenticeship.FirstName} {apprenticeship.LastName}", accountId: commitment.EmployerAccountId, providerId: commitment.ProviderId, commitmentId: commitmentId);
                        apprenticeship.CommitmentId = commitmentId;
                        await _apprenticeshipTransactions.CreateApprenticeship(connection, trans, apprenticeship);
                    }

                    await _historyTransactions.CreateCommitment(
                        connection, trans,
                        new CommitmentHistoryItem
                        {
                            CommitmentId = commitmentId,
                            UserId = userId,
                            UpdatedByRole = callerType
                        });
                    trans.Commit();
                    return commitmentId;
                }
            });
        }

        public async Task<Commitment> GetCommitmentById(long id)
        {
            return await WithConnection(c => { return GetCommitment(id, c); });
        }

        public async Task<IList<CommitmentSummary>> GetCommitmentsByProvider(long providerId)
        {
            return await GetCommitmentsByIdentifier("ProviderId", providerId);
        }

        public async Task<IList<CommitmentSummary>> GetCommitmentsByEmployer(long accountId)
        {
            return await GetCommitmentsByIdentifier("EmployerAccountId", accountId);
        }

        public async Task UpdateCommitment(
            long commitmentId,
            CommitmentStatus commitmentStatus,
            EditStatus editStatus,
            LastUpdateAction lastAction)
        {
            await WithTransaction(async (connection, trans) =>
                    {
                        await UpdateCommitmentStatus(commitmentId, commitmentStatus, connection, trans);
                        await UpdateEditStatus(commitmentId, editStatus, connection, trans);
                        await UpdateLastAction(commitmentId, lastAction, connection, trans);

                        // ToDo: Need to tests
                        var changeType = CommitmentChangeType.SendForReview;
                        if (editStatus == EditStatus.Both && lastAction.LastAction == LastAction.Approve)
                            changeType = CommitmentChangeType.FinalApproval;
                        else if(lastAction.LastAction == LastAction.Approve)
                            changeType = CommitmentChangeType.SendForApproval;

                        await _historyTransactions.UpdateCommitment(connection, trans, changeType,
                            new CommitmentHistoryItem { CommitmentId = commitmentId, UpdatedByRole = lastAction.Caller.CallerType, UserId = lastAction.UserId});
                        return 1L;
                    });
        }

        private static string GetUpdateLastActionSql(Caller caller)
        {
            if (caller.CallerType == CallerType.Employer)
                return @"UPDATE [dbo].[Commitment] SET LastAction = @lastAction, LastUpdatedByEmployerName = @lastUpdatedByName, LastUpdatedByEmployerEmail = @lastUpdatedByEmail WHERE Id = @id;";

            return @"UPDATE [dbo].[Commitment] SET LastAction = @lastAction, LastUpdatedByProviderName = @lastUpdatedByName, LastUpdatedByProviderEmail = @lastUpdatedByEmail WHERE Id = @id;";
        }

        public async Task UpdateCommitmentReference(long commitmentId, string hashValue)
        {
            _logger.Debug($"Updating Commitment Reference {hashValue} for commitment {commitmentId}", commitmentId: commitmentId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitmentId, DbType.Int64);
                parameters.Add("@reference", hashValue, DbType.String);

                var returnCode = await connection.ExecuteAsync(
                    sql: "UPDATE [dbo].[Commitment] SET Reference = @reference WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task SetPaymentOrder(long accountId)
        {
            await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@employerAccountId", accountId);

                var returnCode = await c.ExecuteAsync(
                    sql: "[dbo].[SetPaymentOrder]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return returnCode;
            });
        }

        public async Task DeleteCommitment(long commitmentId, CallerType callerType, string userId)
        {
            _logger.Debug($"Deleting commitment {commitmentId}", commitmentId: commitmentId);

            await WithConnection(async connection =>
            {
                using (var tran = connection.BeginTransaction())
                {
                    var returnCode = await connection.ExecuteAsync(
                        sql: "[dbo].[DeleteCommitment]",
                        transaction: tran,
                        commandType: CommandType.StoredProcedure,
                        param: new { @commitmentId = commitmentId }
                    );

                    await _historyTransactions.DeleteCommitment(
                        connection,
                        tran,
                        new CommitmentHistoryItem
                            {
                                CommitmentId = commitmentId,
                                UpdatedByRole = callerType,
                                UserId = userId
                            });
                    tran.Commit();
                    return returnCode;
                }
            });
        }

        public async Task<long> CreateRelationship(Relationship relationship)
        {
            _logger.Debug(
                $"Creating relationship between Provider {relationship.ProviderId}, Employer {relationship.EmployerAccountId}, Legal Entity: {relationship.LegalEntityId}");

            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ProviderId", relationship.ProviderId, DbType.Int64);
                parameters.Add("@ProviderName", relationship.ProviderName, DbType.String);
                parameters.Add("@LegalEntityId", relationship.LegalEntityId, DbType.String);
                parameters.Add("@LegalEntityName", relationship.LegalEntityName, DbType.String);
                parameters.Add("@LegalEntityAddress", relationship.LegalEntityAddress, DbType.String);
                parameters.Add("@LegalEntityOrganisationType", relationship.LegalEntityOrganisationType, DbType.Int16);
                parameters.Add("@EmployerAccountId", relationship.EmployerAccountId, DbType.String);
                parameters.Add("@Verified", relationship.Verified, DbType.Boolean);

                return await connection.ExecuteAsync(
                    sql: "[dbo].[CreateRelationship]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });
        }

        public async Task<Relationship> GetRelationship(long employerAccountId, long providerId, string legalEntityCode)
        {
            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployerAccountId", employerAccountId);
                parameters.Add("@ProviderId", providerId);
                parameters.Add("@LegalEntityId", legalEntityCode);

                var results = await connection.QueryAsync<Relationship>(
                    sql: $"[dbo].[GetRelationship]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return results.FirstOrDefault();
            });
        }

        public async Task VerifyRelationship(long employerAccountId, long providerId, string legalEntityCode, bool verified)
        {
            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployerAccountId", employerAccountId);
                parameters.Add("@ProviderId", providerId);
                parameters.Add("@LegalEntityId", legalEntityCode);
                parameters.Add("@Verified", verified);

                return await connection.ExecuteAsync(
                    sql: $"[dbo].[VerifyRelationship]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });
        }

        private static async Task<Commitment> GetCommitment(long commitmentId, IDbConnection connection, IDbTransaction transation = null)
        {
            var lookup = new Dictionary<object, Commitment>();
            var mapper = new ParentChildrenMapper<Commitment, Apprenticeship>();

            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", commitmentId);

            var results = await connection.QueryAsync(
                sql: $"[dbo].[GetCommitment]",
                param: parameters,
                transaction: transation,
                commandType: CommandType.StoredProcedure,
                map: mapper.Map(lookup, x => x.Id, x => x.Apprenticeships));

            return lookup.Values.SingleOrDefault();
        }

        private Task<IList<CommitmentSummary>> GetCommitmentsByIdentifier(string identifierName, long identifierValue)
        {
            return WithConnection<IList<CommitmentSummary>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@id", identifierValue);

                var results = await c.QueryAsync<CommitmentSummary>(
                    sql: $"SELECT * FROM [dbo].[CommitmentSummary] WHERE {identifierName} = @id AND CommitmentStatus <> {(int) CommitmentStatus.Deleted};",
                    param: parameters);

                return results.ToList();
            });
        }

        private async Task UpdateCommitmentStatus(long commitmentId, CommitmentStatus commitmentStatus, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.Debug($"Updating commitment {commitmentId} commitment status to {commitmentStatus}", commitmentId: commitmentId);

            var parameters = new DynamicParameters();
            parameters.Add("@id", commitmentId, DbType.Int64);
            parameters.Add("@commitmentStatus", commitmentStatus, DbType.Int16);

            await connection.ExecuteAsync(
                sql: "UPDATE [dbo].[Commitment] SET CommitmentStatus = @commitmentStatus WHERE Id = @id;",
                param: parameters,
                transaction: transaction,
                commandType: CommandType.Text);
        }

        private async Task UpdateEditStatus(long commitmentId, EditStatus editStatus, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.Debug($"Updating commitment {commitmentId} edit status to {editStatus}", commitmentId: commitmentId);


            var parameters = new DynamicParameters();
            parameters.Add("@id", commitmentId, DbType.Int64);
            parameters.Add("@editStatus", editStatus, DbType.Int16);

            await connection.ExecuteAsync(
                sql: "UPDATE [dbo].[Commitment] SET EditStatus = @editStatus WHERE Id = @id;",
                param: parameters,
                transaction: transaction,
                commandType: CommandType.Text);
        }

        private async Task UpdateLastAction(long commitmentId, LastUpdateAction lastUpdateAction, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.Debug($"Updating commitment {commitmentId} last action to {lastUpdateAction.LastAction}", commitmentId: commitmentId);
            var parameters = new DynamicParameters();
            parameters.Add("@id", commitmentId, DbType.Int64);
            parameters.Add("@lastAction", lastUpdateAction.LastAction, DbType.Int16);
            parameters.Add("@lastUpdatedByName", lastUpdateAction.LastUpdaterName, DbType.String);
            parameters.Add("@lastUpdatedByEmail", lastUpdateAction.LastUpdaterEmail, DbType.String);

            await connection.ExecuteAsync(
                sql: GetUpdateLastActionSql(lastUpdateAction.Caller),
                param: parameters,
                transaction: transaction,
                commandType: CommandType.Text);
        }
    }
}
