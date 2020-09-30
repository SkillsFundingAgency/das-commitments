using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Sql.Client;
using SFA.DAS.Sql.Dapper;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using Message = SFA.DAS.Commitments.Domain.Entities.Message;
using TransferApprovalStatus = SFA.DAS.Commitments.Domain.Entities.TransferApprovalStatus;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class CommitmentRepository : BaseRepository, ICommitmentRepository
    {
        private readonly ICommitmentsLogger _logger;
        private readonly ICurrentDateTime _currentDateTime;

        public CommitmentRepository(string databaseConnectionString,
            ICommitmentsLogger logger,
            ICurrentDateTime currentDateTime) : base(databaseConnectionString,
            logger.BaseLogger)
        {
            _logger = logger;
            _currentDateTime = currentDateTime;
        }

        public async Task<long> Create(Commitment commitment)
        {
            _logger.Debug($"Creating commitment with ref: {commitment.Reference}", accountId: commitment.EmployerAccountId, providerId: commitment.ProviderId);

            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@reference", commitment.Reference, DbType.String);
                parameters.Add("@transferSenderId", commitment.TransferSenderId, DbType.Int64);
                parameters.Add("@transferSenderName", commitment.TransferSenderName, DbType.String);
                parameters.Add("@legalEntityId", commitment.LegalEntityId, DbType.String);
                parameters.Add("@legalEntityName", commitment.LegalEntityName, DbType.String);
                parameters.Add("@LegalEntityAddress", commitment.LegalEntityAddress, DbType.String);
                parameters.Add("@legalEntityOrganisationType", commitment.LegalEntityOrganisationType, DbType.Int16);
                parameters.Add("@accountLegalEntityPublicHashedId", commitment.AccountLegalEntityPublicHashedId, DbType.String);
                parameters.Add("@accountId", commitment.EmployerAccountId, DbType.Int64);
                parameters.Add("@providerId", commitment.ProviderId, DbType.Int64);
                parameters.Add("@providerName", commitment.ProviderName, DbType.String);
                parameters.Add("@commitmentStatus", commitment.CommitmentStatus, DbType.Int16);
                parameters.Add("@editStatus", commitment.EditStatus, DbType.Int16);
                parameters.Add("@id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                parameters.Add("@createdOn", _currentDateTime.Now, DbType.DateTime);
                parameters.Add("@lastAction", commitment.LastAction, DbType.Int16);
                parameters.Add("@lastUpdateByEmployerName", commitment.LastUpdatedByEmployerName, DbType.String);
                parameters.Add("@lastUpdateByEmployerEmail", commitment.LastUpdatedByEmployerEmail, DbType.String);
                parameters.Add("@Originator", commitment.Originator, DbType.Byte);

                using (var trans = connection.BeginTransaction())
                {
                    var commitmentId = (await connection.QueryAsync<long>(
                        @"INSERT INTO [dbo].[Commitment](Reference, LegalEntityId, LegalEntityName, LegalEntityAddress, LegalEntityOrganisationType, AccountLegalEntityPublicHashedId,
                        EmployerAccountId, ProviderId, ProviderName, CommitmentStatus, EditStatus, CreatedOn, LastAction, LastUpdatedByEmployerName,
                        LastUpdatedByEmployerEmail, TransferSenderId, TransferSenderName, Originator)
                        VALUES (@reference, @legalEntityId, @legalEntityName, @legalEntityAddress, @legalEntityOrganisationType, @accountLegalEntityPublicHashedId,
                        @accountId, @providerId, @providerName, @commitmentStatus, @editStatus, @createdOn, @lastAction, @lastUpdateByEmployerName,
                        @lastUpdateByEmployerEmail, @TransferSenderId, @TransferSenderName, @Originator);
                        SELECT CAST(SCOPE_IDENTITY() as int);",
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: trans)).Single();

                    trans.Commit();
                    return commitmentId;
                }
            });
        }

        public async Task<Commitment> GetCommitmentById(long id)
        {
            return await WithConnection(c => GetCommitment(id, c));
        }

        public async Task<IList<CommitmentSummary>> GetCommitmentsByProvider(long providerId)
        {
            return await GetCommitmentsByIdentifier("ProviderId", providerId);
        }

        public async Task<IList<CommitmentSummary>> GetCommitmentsByEmployer(long accountId)
        {
            return await GetCommitmentsByIdentifier("EmployerAccountId", accountId);
        }

        public Task<IList<CommitmentAgreement>> GetCommitmentAgreementsForProvider(long providerId)
        {
            return WithConnection<IList<CommitmentAgreement>>(async c =>
            {
                // we only want to get commitments that are approved
                var results = await c.QueryAsync<CommitmentAgreement>(
$@"SELECT Reference,
ale.[Name] as 'LegalEntityName', ale.[PublicHashedId] as 'AccountLegalEntityPublicHashedId'
FROM [dbo].[Commitment]
JOIN [dbo].[AccountLegalEntities] ale on ale.Id = Commitment.AccountLegalEntityId
WHERE ProviderId = @id
AND IsDeleted = 0
AND EditStatus = {(int) EditStatus.Both}
AND (TransferApprovalStatus is null OR TransferApprovalStatus = {(int)TransferApprovalStatus.TransferApproved});",
                    param: new {@id = providerId});

                return results.ToList();
            });
        }

        public async Task UpdateCommitment(Commitment commitment)
        {
            await WithTransaction(async (connection, transaction) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitment.Id, DbType.Int64);
                parameters.Add("@commitmentStatus", commitment.CommitmentStatus, DbType.Int16);
                parameters.Add("@editStatus", commitment.EditStatus, DbType.Int16);
                parameters.Add("@lastAction", commitment.LastAction, DbType.Int16);
                parameters.Add("@transferApprovalStatus", commitment.TransferApprovalStatus, DbType.Int16);
                parameters.Add("@lastUpdatedByEmployerName", commitment.LastUpdatedByEmployerName, DbType.String);
                parameters.Add("@lastUpdatedByEmployerEmail", commitment.LastUpdatedByEmployerEmail, DbType.String);
                parameters.Add("@lastUpdatedByProviderName", commitment.LastUpdatedByProviderName, DbType.String);
                parameters.Add("@lastUpdatedByProviderEmail", commitment.LastUpdatedByProviderEmail, DbType.String);
                parameters.Add("@apprenticeshipEmployerTypeOnApproval", commitment.ApprenticeshipEmployerTypeOnApproval, DbType.Byte);

                await connection.ExecuteAsync(
                    sql: "UpdateCommitment",
                    param: parameters,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            });
        }

        public async Task DeleteCommitment(long commitmentId)
        {
            _logger.Debug($"Deleting commitment {commitmentId}", commitmentId: commitmentId);

            //todo: await WithTransaction(async (connection, transaction) => ??
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

                    tran.Commit();
                    return returnCode;
                }
            });
        }



        public Task<TransferRequest> GetTransferRequest(long transferRequestId)
        {
            return WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@transferRequestId", transferRequestId);

                var results = await c.QueryAsync<TransferRequest>(
                    sql: $"[dbo].[GetTransferRequest]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return results.FirstOrDefault();
            });
        }

        public Task<IList<TransferRequestSummary>> GetTransferRequestsForSender(long transferSenderAccountId)
        {
            return WithConnection<IList<TransferRequestSummary>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@senderEmployerAccountId", transferSenderAccountId);

                var results = await c.QueryAsync<TransferRequestSummary>(
                    sql: $"[dbo].[GetTransferRequestsForSender]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return results.ToList();
            });
        }

        public Task<IList<TransferRequestSummary>> GetPendingTransferRequests()
        {
            return WithConnection<IList<TransferRequestSummary>>(async c =>
            {
                var parameters = new DynamicParameters();

                var results = await c.QueryAsync<TransferRequestSummary>(
                    sql: $"[dbo].[GetPendingTransferRequests]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return results.ToList();
            });
        }

        public Task<IList<TransferRequestSummary>> GetTransferRequestsForReceiver(long transferReceiverAccountId)
        {
            return WithConnection<IList<TransferRequestSummary>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@receiverEmployerAccountId", transferReceiverAccountId);

                var results = await c.QueryAsync<TransferRequestSummary>(
                    sql: $"[dbo].[GetTransferRequestsForReceiver]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return results.ToList();
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

            var commitment = lookup.Values.SingleOrDefault();
            if (commitment != null)
            {
                commitment.Messages = await GetMessages(connection, commitmentId);
            }
            return commitment;
        }

        private Task<IList<CommitmentSummary>> GetCommitmentsByIdentifier(string identifierName, long identifierValue)
        {
            var lookup = new Dictionary<object, CommitmentSummary>();
            var mapper = new ParentChildrenMapper<CommitmentSummary, Message>();

            return WithConnection<IList<CommitmentSummary>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", identifierValue);

                await c.QueryAsync(
                    sql: $"SELECT * FROM [dbo].[CommitmentSummaryWithMessages] WHERE {identifierName} = @id AND CommitmentStatus <> {(int)CommitmentStatus.Deleted} ORDER BY CreatedOn DESC;",
                    param: parameters,
                    map: mapper.Map(lookup, x => x.Id, x => x.Messages),
                    splitOn: "CommitmentId");

                return lookup.Values.ToList();
            });
        }

        private static async Task<List<Message>> GetMessages(IDbConnection connection, long commitmentId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", commitmentId);

            var results = await connection.QueryAsync<Message>(
                sql: $"SELECT * FROM [dbo].[Message] WHERE CommitmentId = @commitmentId",
                param: parameters);

            return results.ToList();
        }
    }
}
