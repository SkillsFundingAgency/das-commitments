using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Sql.Client;
using SFA.DAS.Sql.Dapper;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class CommitmentRepository : BaseRepository, ICommitmentRepository
    {
        private readonly ICommitmentsLogger _logger;
        private readonly ICurrentDateTime _currentDateTime;

        public CommitmentRepository(string databaseConnectionString, ICommitmentsLogger logger, ICurrentDateTime currentDateTime) : base(databaseConnectionString, logger.BaseLogger)
        {
            _logger = logger;
            _currentDateTime = currentDateTime;
        }

        public async Task<long> Create(Commitment commitment)
        {
            _logger.Debug($"Creating commitment with ref: {commitment.Reference}", accountId: commitment.EmployerAccountId, providerId: commitment.ProviderId);

            return await WithConnection(async connection =>
            {
                long commitmentId;

                var parameters = new DynamicParameters();
                parameters.Add("@reference", commitment.Reference, DbType.String);
                parameters.Add("@transferSenderId", commitment.TransferSenderId, DbType.Int64);
                parameters.Add("@transferSenderName", commitment.TransferSenderName, DbType.String);
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
                parameters.Add("@createdOn", _currentDateTime.Now, DbType.DateTime);
                parameters.Add("@lastAction", commitment.LastAction, DbType.Int16);
                parameters.Add("@lastUpdateByEmployerName", commitment.LastUpdatedByEmployerName, DbType.String);
                parameters.Add("@lastUpdateByEmployerEmail", commitment.LastUpdatedByEmployerEmail, DbType.String);

                using (var trans = connection.BeginTransaction())
                {
                    commitmentId = (await connection.QueryAsync<long>(
                        sql:
                        "INSERT INTO [dbo].[Commitment](Reference, LegalEntityId, LegalEntityName, LegalEntityAddress, LegalEntityOrganisationType, " +
                            "EmployerAccountId, ProviderId, ProviderName, CommitmentStatus, EditStatus, CreatedOn, LastAction, LastUpdatedByEmployerName, " +
                            "LastUpdatedByEmployerEmail, TransferSenderId, TransferSenderName) " +
                        "VALUES (@reference, @legalEntityId, @legalEntityName, @legalEntityAddress, @legalEntityOrganisationType, " +
                            "@accountId, @providerId, @providerName, @commitmentStatus, @editStatus, @createdOn, @lastAction, @lastUpdateByEmployerName, " +
                            "@lastUpdateByEmployerEmail, @TransferSenderId, @TransferSenderName); " +
                        "SELECT CAST(SCOPE_IDENTITY() as int);",
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

                await connection.ExecuteAsync(
                    sql: "UpdateCommitment",
                    param: parameters,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            });
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

        public async Task DeleteCommitment(long commitmentId)
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

                    tran.Commit();
                    return returnCode;
                }
            });
        }

        public async Task SetTransferRequestApproval(long transferRequestId, long commitmentId, TransferApprovalStatus transferApprovalStatus, string userEmail, string userName)
        {
            _logger.Debug($"Setting TransferRequest Approval to {transferApprovalStatus} on commitment {commitmentId}", commitmentId: commitmentId);
            try
            {
                await WithConnection(async connection =>
                {
                    using (var tran = connection.BeginTransaction())
                    {
                        var count = await connection.ExecuteAsync(
                            sql: "[dbo].[SetTransferRequestApproval]",
                            transaction: tran,
                            commandType: CommandType.StoredProcedure,
                            param: new
                            {
                                @transferRequestId = transferRequestId,
                                @commitmentId = commitmentId,
                                @transferApprovalStatus = transferApprovalStatus,
                                @transferStatusSetByEmployerName = userName,
                                @transferStatusSetByEmployerEmail = userEmail
                            }
                        );
                        tran.Commit();
                        return count;
                    }
                });
            }
            catch (Exception e)
            {
                if (e.InnerException is SqlException)
                {
                    throw new BadRequestException(e.InnerException.Message, e);
                }
                throw;
            }
        }

        public async Task<long> StartTransferRequestApproval(long commitmentId, decimal cost, List<TrainingCourseSummary> trainingCourses)
        {
            _logger.Debug($"Starting a Transfer Request Approval", commitmentId: commitmentId);
            try
            {
                long transferRequestId = 0;
                var parameters = new DynamicParameters();
                parameters.Add("@commitmentId", commitmentId, DbType.Int64);
                parameters.Add("@cost", cost, DbType.Decimal);
                parameters.Add("@trainingCourses", JsonConvert.SerializeObject(trainingCourses), DbType.String);
                parameters.Add("@transferRequestId", transferRequestId, DbType.Int64, ParameterDirection.Output);

                return await WithConnection(async connection =>
                {
                    using (var tran = connection.BeginTransaction())
                    {
                        await connection.ExecuteAsync(
                            sql: "[dbo].[StartATransferRequest]",
                            transaction: tran,
                            commandType: CommandType.StoredProcedure,
                            param: parameters
                        );
                        tran.Commit();
                        transferRequestId = parameters.Get<long>("@transferRequestId");
                        return transferRequestId;
                    }
                });
            }
            catch (Exception e)
            {
                if (e.InnerException is SqlException)
                {
                    throw new BadRequestException(e.InnerException.Message, e);
                }
                throw;
            }
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

        public async Task ResetEditStatusToEmployer(long commitmentId)
        {
            _logger.Debug($"Resetting edit status of commitment {commitmentId} to Employer", commitmentId: commitmentId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitmentId, DbType.Int64);
                parameters.Add("@editStatus", EditStatus.EmployerOnly, DbType.Int32);

                var returnCode = await connection.ExecuteAsync(
                    sql: "UPDATE [dbo].[Commitment] SET EditStatus=@editStatus WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
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
                parameters.Add("@CreatedOn", _currentDateTime.Now, DbType.DateTime);

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

        public async Task SaveMessage(long commitmentId, Message message)
        {
            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CommitmentId", commitmentId);
                parameters.Add("@Author", message.Author);
                parameters.Add("@Text", message.Text);
                parameters.Add("@CreatedBy", message.CreatedBy);
                parameters.Add("@CreatedDateTime", _currentDateTime.Now, DbType.DateTime);

                return await connection.ExecuteAsync(
                    sql: $"[dbo].[CreateMessage]",
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
