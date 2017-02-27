using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class CommitmentRepository : BaseRepository, ICommitmentRepository
    {
        private readonly ICommitmentsLogger _logger;

        public CommitmentRepository(string databaseConnectionString, ICommitmentsLogger logger) : base(databaseConnectionString)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public async Task<long> Create(Commitment commitment)
        {
            _logger.Debug($"Creating commitment with ref: {commitment.Reference}", accountId: commitment.EmployerAccountId, providerId: commitment.ProviderId);

            return await WithConnection(async connection =>
            {
                long commitmentId;

                var parameters = new DynamicParameters();
                parameters.Add("@reference", commitment.Reference, DbType.String);
                parameters.Add("@legalEntityId", commitment.LegalEntityId, DbType.String);
                parameters.Add("@legalEntityName", commitment.LegalEntityName, DbType.String);
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
                        "INSERT INTO [dbo].[Commitment](Reference, LegalEntityId, LegalEntityName, EmployerAccountId, ProviderId, ProviderName, CommitmentStatus, EditStatus, CreatedOn, LastAction, LastUpdatedByEmployerName, LastUpdatedByEmployerEmail) " +
                        "VALUES (@reference, @legalEntityId, @legalEntityName, @accountId, @providerId, @providerName, @commitmentStatus, @editStatus, @createdOn, @lastAction, @lastUpdateByEmployerName, @lastUpdateByEmployerEmail); " +
                        "SELECT CAST(SCOPE_IDENTITY() as int);",
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: trans)).Single();

                    foreach (var apprenticeship in commitment.Apprenticeships)
                    {
                        _logger.Debug($"Creating apprenticeship in new commitment - {apprenticeship.FirstName} {apprenticeship.LastName}", accountId: commitment.EmployerAccountId, providerId: commitment.ProviderId, commitmentId: commitmentId);
                        apprenticeship.CommitmentId = commitmentId;
                        await CreateApprenticeship(connection, trans, apprenticeship);
                    }

                    trans.Commit();
                }
                return commitmentId;
            });
        }

        public async Task<long> CreateApprenticeship(Apprenticeship apprenticeship)
        {
            _logger.Debug($"Creating apprenticeship - {apprenticeship.FirstName} {apprenticeship.LastName}", accountId: apprenticeship.EmployerAccountId, providerId: apprenticeship.ProviderId, commitmentId: apprenticeship.CommitmentId);
            return await WithConnection(async connection => { return await CreateApprenticeship(connection, null, apprenticeship); });
        }

        public async Task<Commitment> GetCommitmentById(long id)
        {
            return await WithConnection(c => { return GetCommitment(id, c); });
        }

        public async Task<IList<Apprenticeship>> GetApprenticeshipsByEmployer(long accountId)
        {
            return await GetApprenticeshipsByIdentifier("EmployerAccountId", accountId);
        }

        public async Task<IList<Apprenticeship>> GetApprenticeshipsByProvider(long providerId)
        {
            return await GetApprenticeshipsByIdentifier("ProviderId", providerId);
        }

        public async Task<IList<CommitmentSummary>> GetCommitmentsByEmployer(long accountId)
        {
            return await GetCommitmentsByIdentifier("EmployerAccountId", accountId);
        }

        public async Task<IList<CommitmentSummary>> GetCommitmentsByProvider(long providerId)
        {
            return await GetCommitmentsByIdentifier("ProviderId", providerId);
        }

        public async Task UpdateCommitmentStatus(long commitmentId, CommitmentStatus commitmentStatus)
        {
            _logger.Debug($"Updating commitment {commitmentId} commitment status to {commitmentStatus}", commitmentId: commitmentId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitmentId, DbType.Int64);
                parameters.Add("@commitmentStatus", commitmentStatus, DbType.Int16);

                var returnCode = await connection.ExecuteAsync(
                    sql: "UPDATE [dbo].[Commitment] SET CommitmentStatus = @commitmentStatus WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateEditStatus(long commitmentId, EditStatus editStatus)
        {
            _logger.Debug($"Updating commitment {commitmentId} edit status to {editStatus}", commitmentId: commitmentId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitmentId, DbType.Int64);
                parameters.Add("@editStatus", editStatus, DbType.Int16);

                var returnCode = await connection.ExecuteAsync(
                    sql: "UPDATE [dbo].[Commitment] SET EditStatus = @editStatus WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateLastAction(long commitmentId, LastAction lastAction, Caller caller, string updatedByName, string updatedByEmailAddress)
        {
            _logger.Debug($"Updating commitment {commitmentId} last action to {lastAction}", commitmentId: commitmentId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitmentId, DbType.Int64);
                parameters.Add("@lastAction", lastAction, DbType.Int16);
                parameters.Add("@lastUpdatedByName", updatedByName, DbType.String);
                parameters.Add("@lastUpdatedByEmail", updatedByEmailAddress, DbType.String);

                var returnCode = await connection.ExecuteAsync(
                    sql: GetUpdateLastActionSql(caller),
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        private static string GetUpdateLastActionSql(Caller caller)
        {
            if (caller.CallerType == CallerType.Employer)
                return @"UPDATE [dbo].[Commitment] SET LastAction = @lastAction, LastUpdatedByEmployerName = @lastUpdatedByName, LastUpdatedByEmployerEmail = @lastUpdatedByEmail WHERE Id = @id;";

            return @"UPDATE [dbo].[Commitment] SET LastAction = @lastAction, LastUpdatedByProviderName = @lastUpdatedByName, LastUpdatedByProviderEmail = @lastUpdatedByEmail WHERE Id = @id;";
        }

        public async Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller)
        {
            _logger.Debug($"Updating apprenticeship {apprenticeship.Id}", accountId: apprenticeship.EmployerAccountId, providerId: apprenticeship.ProviderId, commitmentId: apprenticeship.CommitmentId, apprenticeshipId: apprenticeship.Id);

            await WithConnection(async connection =>
            {
                var parameters = GetApprenticeshipUpdateCreateParameters(apprenticeship);
                parameters.Add("@id", apprenticeship.Id, DbType.Int64);

                var sql = GetUpdateApprenticeshipSql(caller.CallerType);

                var returnCode = await connection.ExecuteAsync(
                    sql: sql,
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus)
        {
            _logger.Debug($"Updating apprenticeship {apprenticeshipId} for commitment {commitmentId} payment status to {paymentStatus}", commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@paymentStatus", paymentStatus, DbType.Int16);

                var returnCode = await connection.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[Apprenticeship] SET PaymentStatus = @paymentStatus " +
                    "WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, AgreementStatus agreementStatus)
        {
            _logger.Debug($"Updating apprenticeship {apprenticeshipId} for commitment {commitmentId} agreement status to {agreementStatus}", commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@agreementStatus", agreementStatus, DbType.Int16);
                parameters.Add("@agreedOn", DateTime.UtcNow, DbType.DateTime);

                var returnCode = await connection.ExecuteAsync(
                    "UPDATE [dbo].[Apprenticeship] SET AgreementStatus = @agreementStatus " +
                    "WHERE Id = @id;",
                    parameters,
                    commandType: CommandType.Text);

                if (agreementStatus == AgreementStatus.BothAgreed)
                {
                    returnCode = await connection.ExecuteAsync(
                        "UPDATE [dbo].[Apprenticeship] SET AgreedOn = @agreedOn " +
                        "WHERE Id = @id AND AgreedOn IS NULL;",
                        parameters,
                        commandType: CommandType.Text);
                }

                return returnCode;
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

        public async Task<Apprenticeship> GetApprenticeship(long apprenticeshipId)
        {
            var results = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId);

                return await c.QueryAsync<Apprenticeship>(
                    sql: $"SELECT * FROM [dbo].[ApprenticeshipSummary] WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);
            });

            return results.SingleOrDefault();
        }

        public async Task DeleteApprenticeship(long apprenticeshipId)
        {
            _logger.Debug($"Deleting apprenticeship {apprenticeshipId}", apprenticeshipId: apprenticeshipId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);

                var returnCode = await connection.ExecuteAsync(
                    sql:
                    "DELETE FROM [dbo].[Apprenticeship] " +
                    "WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

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

        public async Task<IList<Apprenticeship>> BulkUploadApprenticeships(long commitmentId, IEnumerable<Apprenticeship> apprenticeships)
        {
            _logger.Debug($"Bulk upload {apprenticeships.Count()} apprenticeships for commitment {commitmentId}", commitmentId: commitmentId);

            var table = BuildApprenticeshipDataTable(apprenticeships);

            var insertedApprenticeships = await WithConnection(x => UploadApprenticeshipsAndGetIds(commitmentId, x, table));

            return insertedApprenticeships;
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

        public async Task VerifyRelationship(long employerAccountId, long providerId, string legalEntityCode)
        {
            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployerAccountId", employerAccountId);
                parameters.Add("@ProviderId", providerId);
                parameters.Add("@LegalEntityId", legalEntityCode);

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

        private async Task<long> CreateApprenticeship(IDbConnection connection, IDbTransaction trans, Apprenticeship apprenticeship)
        {
            var parameters = GetApprenticeshipUpdateCreateParameters(apprenticeship);

            var apprenticeshipId = (await connection.QueryAsync<long>(
                sql:
                "INSERT INTO [dbo].[Apprenticeship](CommitmentId, FirstName, LastName, DateOfBirth, NINumber, ULN, TrainingType, TrainingCode, TrainingName, Cost, StartDate, EndDate, PaymentStatus, AgreementStatus, EmployerRef, ProviderRef, CreatedOn) " +
                "VALUES (@commitmentId, @firstName, @lastName, @dateOfBirth, @niNumber, @uln, @trainingType, @trainingCode, @trainingName, @cost, @startDate, @endDate, @paymentStatus, @agreementStatus, @employerRef, @providerRef, @createdOn); " +
                "SELECT CAST(SCOPE_IDENTITY() as int);",
                param: parameters,
                commandType: CommandType.Text,
                transaction: trans)).Single();

            return apprenticeshipId;
        }

        private static async Task<IList<Apprenticeship>> UploadApprenticeshipsAndGetIds(long commitmentId, IDbConnection x, DataTable table)
        {
            IList<Apprenticeship> apprenticeships;

            using (var tran = x.BeginTransaction()) // TODO: Set Isolation Level
            {
                await x.ExecuteAsync(
                    sql: "[dbo].[BulkUploadApprenticships]",
                    transaction: tran,
                    commandType: CommandType.StoredProcedure,
                    param: new {@commitmentId = commitmentId, @apprenticeships = table.AsTableValuedParameter("dbo.ApprenticeshipTable")}
                );

                var commitment = await GetCommitment(commitmentId, x, tran);
                apprenticeships = commitment.Apprenticeships;

                tran.Commit();
            }

            return apprenticeships.ToList();
        }

        private DataTable BuildApprenticeshipDataTable(IEnumerable<Apprenticeship> apprenticeships)
        {
            var apprenticeshipsTable = CreateApprenticeshipsDataTable();

            foreach (var apprenticeship in apprenticeships)
            {
                AddApprenticeshipToTable(apprenticeshipsTable, apprenticeship);
            }

            return apprenticeshipsTable;
        }

        private static DataTable CreateApprenticeshipsDataTable()
        {
            var apprenticeshipsTable = new DataTable();

            apprenticeshipsTable.Columns.Add("FirstName", typeof(string));
            apprenticeshipsTable.Columns.Add("LastName", typeof(string));
            apprenticeshipsTable.Columns.Add("ULN", typeof(string));
            apprenticeshipsTable.Columns.Add("TrainingType", typeof(int));
            apprenticeshipsTable.Columns.Add("TrainingCode", typeof(string));
            apprenticeshipsTable.Columns.Add("TrainingName", typeof(string));
            apprenticeshipsTable.Columns.Add("Cost", typeof(decimal));
            apprenticeshipsTable.Columns.Add("StartDate", typeof(DateTime));
            apprenticeshipsTable.Columns.Add("EndDate", typeof(DateTime));
            apprenticeshipsTable.Columns.Add("AgreementStatus", typeof(short));
            apprenticeshipsTable.Columns.Add("PaymentStatus", typeof(short));
            apprenticeshipsTable.Columns.Add("DateOfBirth", typeof(DateTime));
            apprenticeshipsTable.Columns.Add("NINumber", typeof(string));
            apprenticeshipsTable.Columns.Add("EmployerRef", typeof(string));
            apprenticeshipsTable.Columns.Add("ProviderRef", typeof(string));
            apprenticeshipsTable.Columns.Add("CreatedOn", typeof(DateTime));
            return apprenticeshipsTable;
        }

        private static DataRow AddApprenticeshipToTable(DataTable apprenticeshipsTable, Apprenticeship apprenticeship)
        {
            var a = apprenticeship;

            return apprenticeshipsTable.Rows.Add(a.FirstName, a.LastName, a.ULN, a.TrainingType, a.TrainingCode, a.TrainingName,
                a.Cost, a.StartDate, a.EndDate, a.AgreementStatus, a.PaymentStatus, a.DateOfBirth, a.NINumber,
                a.EmployerRef, a.ProviderRef, DateTime.UtcNow);
        }

        private static DynamicParameters GetApprenticeshipUpdateCreateParameters(Apprenticeship apprenticeship)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", apprenticeship.CommitmentId, DbType.Int64);
            parameters.Add("@firstName", apprenticeship.FirstName, DbType.String);
            parameters.Add("@lastName", apprenticeship.LastName, DbType.String);
            parameters.Add("@dateOfBirth", apprenticeship.DateOfBirth, DbType.DateTime);
            parameters.Add("@niNumber", apprenticeship.NINumber, DbType.String);
            parameters.Add("@trainingType", apprenticeship.TrainingType, DbType.Int32);
            parameters.Add("@trainingCode", apprenticeship.TrainingCode, DbType.String);
            parameters.Add("@trainingName", apprenticeship.TrainingName, DbType.String);
            parameters.Add("@uln", apprenticeship.ULN, DbType.String);
            parameters.Add("@cost", apprenticeship.Cost, DbType.Decimal);
            parameters.Add("@startDate", apprenticeship.StartDate, DbType.DateTime);
            parameters.Add("@endDate", apprenticeship.EndDate, DbType.DateTime);
            parameters.Add("@paymentStatus", apprenticeship.PaymentStatus, DbType.Int16);
            parameters.Add("@agreementStatus", apprenticeship.AgreementStatus, DbType.Int16);
            parameters.Add("@employerRef", apprenticeship.EmployerRef, DbType.String);
            parameters.Add("@providerRef", apprenticeship.ProviderRef, DbType.String);
            parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);
            return parameters;
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

        private Task<IList<Apprenticeship>> GetApprenticeshipsByIdentifier(string identifierName, long identifierValue)
        {
            return WithConnection<IList<Apprenticeship>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@id", identifierValue);

                var results = await c.QueryAsync<Apprenticeship>(
                    sql: $"SELECT * FROM [dbo].[ApprenticeshipSummary] WHERE {identifierName} = @id AND PaymentStatus <> {(int) PaymentStatus.Deleted};",
                    param: parameters);

                return results.ToList();
            });
        }

        private static string GetUpdateApprenticeshipSql(CallerType callerType)
        {
            var refItem = callerType == CallerType.Employer ? "EmployerRef = @employerRef" : "ProviderRef = @providerRef";

            return "UPDATE [dbo].[Apprenticeship] " +
                   "SET CommitmentId = @commitmentId, FirstName = @firstName, LastName = @lastName, DateOfBirth = @dateOfBirth, NINUmber = @niNumber, " +
                   "ULN = @uln, TrainingType = @trainingType, TrainingCode = @trainingCode, TrainingName = @trainingName, Cost = @cost, " +
                   "StartDate = @startDate, EndDate = @endDate, PaymentStatus = @paymentStatus, AgreementStatus = @agreementStatus, " +
                   $"{refItem} WHERE Id = @id;";
        }
    }
}
