using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data.Transactions;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class ApprenticeshipRepository : BaseRepository, IApprenticeshipRepository
    {
        private readonly ICommitmentsLogger _logger;
        private readonly IApprenticeshipTransactions _apprenticeshipTransactions;

        public ApprenticeshipRepository(string connectionString, ICommitmentsLogger logger, IApprenticeshipTransactions apprenticeshipTransactions) : base(connectionString)
        {
            _logger = logger;
            _apprenticeshipTransactions = apprenticeshipTransactions;
        }

        public async Task<long> CreateApprenticeship(Apprenticeship apprenticeship)
        {
            _logger.Debug($"Creating apprenticeship - {apprenticeship.FirstName} {apprenticeship.LastName}", accountId: apprenticeship.EmployerAccountId, providerId: apprenticeship.ProviderId, commitmentId: apprenticeship.CommitmentId);

            return await WithTransaction(async (connection, trans)=>
                {
                    var apprenticeshipId = await _apprenticeshipTransactions.CreateApprenticeship(connection, trans, apprenticeship);
                    return apprenticeshipId;
                });
        }

        public async Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller)
        {
            _logger.Debug($"Updating apprenticeship {apprenticeship.Id}", accountId: apprenticeship.EmployerAccountId, providerId: apprenticeship.ProviderId, commitmentId: apprenticeship.CommitmentId, apprenticeshipId: apprenticeship.Id);

            await WithTransaction(async (connection, trans) =>
                {
                    var returnCode = await _apprenticeshipTransactions.UpdateApprenticeship(connection, trans, apprenticeship, caller);
                    return returnCode;
            });
        }

        public async Task StopApprenticeship(long commitmentId, long apprenticeshipId, DateTime dateOfChange)
        {
            _logger.Debug($"Stopping apprenticeship {apprenticeshipId} for commitment {commitmentId}", commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await WithTransaction(async (conn, tran) => 
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@paymentStatus", PaymentStatus.Withdrawn, DbType.Int16);
                parameters.Add("@stopDate", dateOfChange, DbType.Date);

                var returnCode = await conn.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[Apprenticeship] SET PaymentStatus = @paymentStatus, StopDate = @stopDate " +
                    "WHERE Id = @id;",
                    transaction: tran,
                    param: parameters,
                    commandType: CommandType.Text);
            });
        }

        public async Task PauseOrResumeApprenticeship(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus)
        {
            if (!(paymentStatus == PaymentStatus.Paused || paymentStatus == PaymentStatus.Active))
                throw new ArgumentException("PaymentStatus should be Paused or Active", nameof(paymentStatus));

            _logger.Debug($"Updating apprenticeship status to {paymentStatus} for appreticeship {apprenticeshipId} for commitment {commitmentId}", commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await WithTransaction(async (conn, tran) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@paymentStatus", paymentStatus, DbType.Int16);

                var returnCode = await conn.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[Apprenticeship] SET PaymentStatus = @paymentStatus " +
                    "WHERE Id = @id;",
                    transaction: tran,
                    param: parameters,
                    commandType: CommandType.Text);
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

        public async Task UpdateApprenticeshipStatuses(List<Apprenticeship> apprenticeships)
        {
            await WithTransaction(async (connection, transaction) =>
            {
                foreach (var apprenticeship in apprenticeships)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@id", apprenticeship.Id, DbType.Int64);
                    parameters.Add("@paymentStatus", apprenticeship.PaymentStatus, DbType.Int16);
                    parameters.Add("@agreementStatus", apprenticeship.AgreementStatus, DbType.Int16);
                    parameters.Add("@agreedOn", apprenticeship.AgreedOn, DbType.DateTime);

                    await connection.ExecuteAsync(
                        sql: "UpdateApprenticeshipStatuses",
                        param: parameters,
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure);
                }
                return 0;
            });
        }

        public async Task DeleteApprenticeship(long apprenticeshipId)
        {
            _logger.Debug($"Deleting apprenticeship {apprenticeshipId}", apprenticeshipId: apprenticeshipId);

            await WithTransaction(async (connection, transactions) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);

                var returnCode = await connection.ExecuteAsync(
                    sql:
                    "DELETE FROM [dbo].[Apprenticeship] " +
                    "WHERE Id = @id;",
                    param: parameters,
                    transaction: transactions,
                    commandType: CommandType.Text);
                return returnCode;
            });
        }

        public async Task<IList<Apprenticeship>> BulkUploadApprenticeships(long commitmentId, IEnumerable<Apprenticeship> apprenticeships)
        {
            _logger.Debug($"Bulk upload {apprenticeships.Count()} apprenticeships for commitment {commitmentId}", commitmentId: commitmentId);

            var table = BuildApprenticeshipDataTable(commitmentId, apprenticeships);

            var insertedApprenticeships =
                await WithConnection(connection => UploadApprenticeshipsAndGetIds(commitmentId, connection, table));

            return insertedApprenticeships;
        }

        public async Task<IList<Apprenticeship>> GetApprenticeshipsByEmployer(long accountId)
        {
            return await GetApprenticeshipsByIdentifier("EmployerAccountId", accountId);
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

        public async Task<IList<Apprenticeship>> GetApprenticeshipsByProvider(long providerId)
        {
            return await GetApprenticeshipsByIdentifier("ProviderId", providerId);
        }

        private static async Task<IList<Apprenticeship>> UploadApprenticeshipsAndGetIds(long commitmentId, SqlConnection x, DataTable table)
        {
            IList<Apprenticeship> apprenticeships;

            using (var tran = x.BeginTransaction())
            {
                await DeleteCommitmentApprenticeships(commitmentId, x, tran);
                BulkCopyApprenticeships(x, table, tran);

                var commitment = await GetCommitment(commitmentId, x, tran);
                apprenticeships = commitment.Apprenticeships;

                tran.Commit();
            }

            return apprenticeships.ToList();
        }

        private static void BulkCopyApprenticeships(SqlConnection x, DataTable table, SqlTransaction tran)
        {
            using (var bulkCopy = new SqlBulkCopy(x, SqlBulkCopyOptions.Default, tran))
            {
                bulkCopy.DestinationTableName = "[dbo].[Apprenticeship]";
                bulkCopy.ColumnMappings.Add("CommitmentId", "CommitmentId");
                bulkCopy.ColumnMappings.Add("FirstName", "FirstName");
                bulkCopy.ColumnMappings.Add("LastName", "LastName");
                bulkCopy.ColumnMappings.Add("ULN", "ULN");
                bulkCopy.ColumnMappings.Add("TrainingType", "TrainingType");
                bulkCopy.ColumnMappings.Add("TrainingCode", "TrainingCode");
                bulkCopy.ColumnMappings.Add("TrainingName", "TrainingName");
                bulkCopy.ColumnMappings.Add("Cost", "Cost");
                bulkCopy.ColumnMappings.Add("StartDate", "StartDate");
                bulkCopy.ColumnMappings.Add("EndDate", "EndDate");
                bulkCopy.ColumnMappings.Add("AgreementStatus", "AgreementStatus");
                bulkCopy.ColumnMappings.Add("PaymentStatus", "PaymentStatus");
                bulkCopy.ColumnMappings.Add("DateOfBirth", "DateOfBirth");
                bulkCopy.ColumnMappings.Add("NINumber", "NINumber");
                bulkCopy.ColumnMappings.Add("EmployerRef", "EmployerRef");
                bulkCopy.ColumnMappings.Add("ProviderRef", "ProviderRef");
                bulkCopy.ColumnMappings.Add("CreatedOn", "CreatedOn");
                bulkCopy.WriteToServer(table);
            }
        }

        private static async Task DeleteCommitmentApprenticeships(long commitmentId, SqlConnection x, SqlTransaction tran)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", commitmentId, DbType.Int64);

            await x.ExecuteAsync(
                sql: "DELETE FROM [dbo].[Apprenticeship] WHERE CommitmentId = @CommitmentId;",
                param: parameters,
                transaction: tran,
                commandType: CommandType.Text);
        }

        private DataTable BuildApprenticeshipDataTable(long commitmentId, IEnumerable<Apprenticeship> apprenticeships)
        {
            var apprenticeshipsTable = CreateApprenticeshipsDataTable();

            foreach (var apprenticeship in apprenticeships)
            {
                AddApprenticeshipToTable(apprenticeshipsTable, commitmentId, apprenticeship);
            }

            return apprenticeshipsTable;
        }

        private static DataTable CreateApprenticeshipsDataTable()
        {
            var apprenticeshipsTable = new DataTable();

            apprenticeshipsTable.Columns.Add("CommitmentId", typeof(long));
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

        private static DataRow AddApprenticeshipToTable(DataTable apprenticeshipsTable, long commitmentId, Apprenticeship a)
        {
            return apprenticeshipsTable.Rows.Add(commitmentId, a.FirstName, a.LastName, a.ULN, a.TrainingType, a.TrainingCode, a.TrainingName,
                a.Cost, a.StartDate, a.EndDate, a.AgreementStatus, a.PaymentStatus, a.DateOfBirth, a.NINumber,
                a.EmployerRef, a.ProviderRef, DateTime.UtcNow);
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

        private Task<IList<Apprenticeship>> GetApprenticeshipsByIdentifier(string identifierName, long identifierValue)
        {
            return WithConnection<IList<Apprenticeship>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@id", identifierValue);

                var results = await c.QueryAsync<Apprenticeship>(
                    sql: $"SELECT * FROM [dbo].[ApprenticeshipSummary] WHERE {identifierName} = @id AND PaymentStatus <> {(int)PaymentStatus.Deleted};",
                    param: parameters);

                return results.ToList();
            });
        }

        public async Task<IList<ApprenticeshipResult>> GetActiveApprenticeshipsByUlns(IEnumerable<string> ulns)
        {
            var ulnDataTable = GenerateUlnDataTable(ulns);

            return await WithConnection(async c =>
            {
                var result = await c.QueryAsync<ApprenticeshipResult>(
                    sql: $"[dbo].[GetActiveApprenticeshipsByULNs]",
                    param: new { @ULNs = ulnDataTable.AsTableValuedParameter("dbo.ULNTable") },
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            });
        }

        private static DataTable GenerateUlnDataTable(IEnumerable<string> ulns)
        {
            var result = new DataTable();

            result.Columns.Add("ULN", typeof(string));

            foreach (var uln in ulns.Where(u => u.Length <= 50))
            {
                result.Rows.Add(uln);
            }

            return result;
        }
    }
}
