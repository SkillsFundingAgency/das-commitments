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
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data.Transactions;
using SFA.DAS.Sql.Client;
using SFA.DAS.Sql.Dapper;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class ApprenticeshipRepository : BaseRepository, IApprenticeshipRepository
    {
        private readonly ICommitmentsLogger _logger;
        private readonly IApprenticeshipTransactions _apprenticeshipTransactions;
        private readonly ICurrentDateTime _currentDateTime;

        public ApprenticeshipRepository(
            string connectionString,
            ICommitmentsLogger logger,
            IApprenticeshipTransactions apprenticeshipTransactions,
            ICurrentDateTime currentDateTime) : base(connectionString, logger.BaseLogger)
        {
            _logger = logger;
            _apprenticeshipTransactions = apprenticeshipTransactions;
            _currentDateTime = currentDateTime;
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

        public async Task StopApprenticeship(long commitmentId, long apprenticeshipId, DateTime dateOfChange, bool? madeRedundant)
        {
            _logger.Debug($"Stopping apprenticeship {apprenticeshipId} for commitment {commitmentId}", commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await WithTransaction(async (conn, tran) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@paymentStatus", PaymentStatus.Withdrawn, DbType.Int16);
                parameters.Add("@stopDate", dateOfChange, DbType.Date);
                parameters.Add("@madeRedundant", madeRedundant, DbType.Byte);

                var returnCode = await conn.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[Apprenticeship] SET PaymentStatus = @paymentStatus, " +
                    "StopDate = @stopDate, " +
                    "MadeRedundant = @madeRedundant " +
                    "WHERE PaymentStatus != 4 AND Id = @id;",
                    transaction: tran,
                    param: parameters,
                    commandType: CommandType.Text);
            });
        }

        public async Task ResumeApprenticeship(long commitmentId, long apprenticeshipId)
        {

            _logger.Debug($"Updating apprenticeship status to {PaymentStatus.Active} for apprenticeship {apprenticeshipId} for commitment {commitmentId}", commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await WithTransaction(async (conn, tran) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@paymentStatus", PaymentStatus.Active, DbType.Int16);

                var returnCode = await conn.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[Apprenticeship] SET PaymentStatus = @paymentStatus, " +
                    "PauseDate = null " +
                    "WHERE PaymentStatus != 4 AND Id = @id;",
                    transaction: tran,
                    param: parameters,
                    commandType: CommandType.Text);
            });
        }

        // note: commitmentId is not used
        public async Task PauseApprenticeship(long commitmentId, long apprenticeshipId, DateTime pauseDate)
        {
            _logger.Debug($"Updating apprenticeship status to {PaymentStatus.Paused} for apprenticeship {apprenticeshipId} for commitment {commitmentId}", commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await WithTransaction(async (conn, tran) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@paymentStatus", PaymentStatus.Paused, DbType.Int16);
                parameters.Add("@pauseDate", pauseDate, DbType.DateTime);

                var returnCode = await conn.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[Apprenticeship] SET PaymentStatus = @paymentStatus, " +
                    "PauseDate = @pauseDate " +
                    "WHERE PaymentStatus != 4 AND Id = @id;",
                    transaction: tran,
                    param: parameters,
                    commandType: CommandType.Text);
            });
        }

        public async Task UpdateApprenticeshipEpa(long apprenticeshipId, string epaOrgId)
        {
            _logger.Info($"Updating apprenticeship {apprenticeshipId} EPAOrgId to {epaOrgId ?? "NULL"}");

            var rowsAffected = await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@EPAOrgId", epaOrgId, DbType.AnsiStringFixedLength);

                return await connection.ExecuteAsync(
                    "UPDATE [dbo].[Apprenticeship] SET EPAOrgId = @EPAOrgId "
                    + "WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);
            });

            if (rowsAffected == 0) // best exception to throw? create new ApprenticeshipNotFound or something?
                throw new ArgumentOutOfRangeException($"Apprenticeship with Id {apprenticeshipId} not found");
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
                    "WHERE PaymentStatus != 4 AND Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateApprenticeshipStopDate(long commitmentId, long apprenticeshipId, DateTime stopDate)
        {
            _logger.Debug($"Updating apprenticeship {apprenticeshipId} for commitment {commitmentId} stop date to {stopDate}", commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@stopDate", stopDate, DbType.DateTime);

                return await connection.ExecuteAsync(
                     "UPDATE [dbo].[Apprenticeship] SET StopDate = @stopDate " +
                     "WHERE PaymentStatus != 4 AND Id = @id;",
                     parameters,
                     commandType: CommandType.Text);
            });
        }

        public async Task<IEnumerable<long>> GetEmployerAccountIds()
        {
            var results = await WithConnection(async c => await c.QueryAsync<long>(
                sql: "SELECT DISTINCT EmployerAccountId FROM [dbo].[Commitment]",
                commandType: CommandType.Text));

            return results;
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

        public async Task<ApprenticeshipsResult> GetApprenticeshipsByEmployer(long accountId, string searchKeyword)
        {
            return await GetApprenticeshipsByIdentifier("@employerId", accountId, searchKeyword);
        }

        public async Task<Apprenticeship> GetApprenticeship(long apprenticeshipId)
        {
            return await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId);
                parameters.Add("@now", _currentDateTime.Now);

                const string sql = "GetApprenticeshipWithPriceHistory";

                Apprenticeship result = null;

                await c.QueryAsync<Apprenticeship, PriceHistory, Apprenticeship>
                (sql, (apprenticeship, history) =>
                {
                    if (result == null)
                    {
                        result = apprenticeship;
                    }

                    if (history.ApprenticeshipId != 0)
                    {
                        result.PriceHistory.Add(history);
                    }

                    return apprenticeship;
                }, parameters, commandType: CommandType.StoredProcedure);

                return result;
            });
        }

        public async Task InsertPriceHistory(long apprenticeshipId, IEnumerable<PriceHistory> priceHistory)
        {
            // -> Delete all price episodes from apprenticeshipId
            // -> Insert all new ones

            await WithTransaction(
                async (c, t) =>
                    {
                        await c.ExecuteAsync(
                            sql: $"DELETE FROM [dbo].[PriceHistory] WHERE ApprenticeshipId = {apprenticeshipId};",
                            commandType: CommandType.Text,
                            transaction: t);

                        foreach (var item in priceHistory)
                        {
                            var parameters = new DynamicParameters();
                            parameters.Add("@apprenticeshipId", apprenticeshipId, DbType.Int64);
                            parameters.Add("@cost", item.Cost, DbType.Decimal);
                            parameters.Add("@fromDate", item.FromDate, DbType.DateTime);
                            parameters.Add("@toDate", item.ToDate, DbType.DateTime);

                            await
                                c.ExecuteAsync(
                                    sql:
                                        "INSERT INTO [dbo].[PriceHistory](ApprenticeshipId, Cost, FromDate, ToDate) "
                                        + "VALUES (@apprenticeshipId, @cost, @fromDate, @toDate);",
                                    param: parameters,
                                    commandType: CommandType.Text,
                                    transaction: t);
                        }
                    }
                );
        }

        public async Task<IEnumerable<ChangeOfPartyRequest>> GetChangeOfPartyResponse(long apprenticeshipId)
        {
            var results = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@apprenticeshipId", apprenticeshipId);

                return await c.QueryAsync<ChangeOfPartyRequest>(
                    sql: @"SELECT  [changeOfPartyRequest].Id
                          ,[ApprenticeshipId]
                          ,[ChangeOfPartyType]
                          ,[OriginatingParty]
                          ,[changeOfPartyRequest].[ProviderId]
                          ,[Price]
                          ,[StartDate]
                          ,[EndDate]
                          ,[Status]
                          ,[CohortId]
                          ,[NewApprenticeshipId]
                          , commitment.WithParty
                          FROM[dbo].[ChangeOfPartyRequest] changeOfPartyRequest
                          LEFT JOIN[dbo].[Commitment] commitment on commitment.Id = changeOfPartyRequest.CohortId 
                          WHERE ApprenticeshipId = @apprenticeshipId;",
                    param: parameters,
                    commandType: CommandType.Text);
            });

            return results;
        }

        public async Task<IEnumerable<PriceHistory>> GetPriceHistory(long apprenticeshipId)
        {
            var results = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@apprenticeshipId", apprenticeshipId);

                return await c.QueryAsync<PriceHistory>(
                    sql: "SELECT * FROM [dbo].[PriceHistory] WHERE ApprenticeshipId = @apprenticeshipId;",
                    param: parameters,
                    commandType: CommandType.Text);
            });

            return results;
        }

        public async Task CreatePriceHistoryForApprenticeshipsInCommitment(long commitmentId)
        {
            await WithTransaction(
                async (connection, transaction) =>
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@commitmentId", commitmentId, DbType.Int64);

                        await
                            connection.ExecuteAsync(
                                sql:
                                    "INSERT INTO [dbo].[PriceHistory] (ApprenticeshipId, Cost, FromDate) "
                                    + "SELECT Id, Cost, StartDate FROM [dbo].[Apprenticeship] "
                                    + "WHERE CommitmentId = @commitmentId "
                                    + "AND Id NOT IN(SELECT ApprenticeshipId FROM [dbo].[PriceHistory])",
                                param: parameters,
                                transaction: transaction,
                                commandType: CommandType.Text);
                    });
        }

        public async Task<IList<AlertSummary>> GetEmployerApprenticeshipAlertSummary()
        {
            return await WithConnection(async connection =>
            {
                var results = await connection.QueryAsync<AlertSummary>(
                    sql: "[dbo].[GetAlertsSummary]",
                    commandType: CommandType.StoredProcedure);

                return results.ToList();
            });
        }

        public async Task<IList<ProviderAlertSummary>> GetProviderApprenticeshipAlertSummary()
        {
            return await WithConnection(async connection =>
            {
                var results = await connection.QueryAsync<ProviderAlertSummary>(
                    sql: "[dbo].[GetProviderAlertsSummary]",
                    commandType: CommandType.StoredProcedure);

                return results.ToList();
            });
        }

        public async Task SetHasHadDataLockSuccess(long apprenticeshipId)
        {
            _logger.Debug($"Setting HasHadDataLockSuccess for apprenticeship {apprenticeshipId}", apprenticeshipId: apprenticeshipId);

            await WithTransaction(async (conn, tran) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@apprenticeshipId", apprenticeshipId, DbType.Int64);
                parameters.Add("@hasHadDataLockSuccess", 1, DbType.Boolean);

                var returnCode = await conn.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[Apprenticeship] SET HasHadDataLockSuccess = @hasHadDataLockSuccess " +
                    "WHERE Id = @apprenticeshipId;",
                    transaction: tran,
                    param: parameters,
                    commandType: CommandType.Text);
            });
        }

        public async Task<ApprenticeshipsResult> GetApprenticeshipsByUln(string uln, long accountId)
        {
            return await WithConnection(async c =>
             {
                 var parameters = new DynamicParameters();
                 parameters.Add("@ULN", uln, DbType.String);
                 parameters.Add("@accountId", accountId, DbType.Int64);

                 const string sql = "[GetApprenticeshipsByULN]";

                 var apprenticeships = (await c.QueryAsync<Apprenticeship>(sql, parameters, commandType: CommandType.StoredProcedure))
                     .ToList();

                 return new ApprenticeshipsResult
                 {
                     Apprenticeships = apprenticeships,
                     TotalCount = apprenticeships.Count
                 };
             });
        }

        private static async Task<IList<Apprenticeship>> UploadApprenticeshipsAndGetIds(long commitmentId, SqlConnection x, DataTable table)
        {
            IList<Apprenticeship> apprenticeships;

            using (var tran = x.BeginTransaction())
            {
                await DeleteCommitmentApprenticeships(commitmentId, x, tran);
                await ResetCohortApprovals(commitmentId, x, tran);
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
                bulkCopy.ColumnMappings.Add("ReservationId", "ReservationId");
                bulkCopy.ColumnMappings.Add("Email", "Email");
                bulkCopy.WriteToServer(table);
            }
        }

        private static async Task ResetCohortApprovals(long commitmentId, SqlConnection x, SqlTransaction tran)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", commitmentId, DbType.Int64);

            await x.ExecuteAsync(
                sql: "UPDATE Commitment Set [Approvals]=0 WHERE Id = @CommitmentId;",
                param: parameters,
                transaction: tran,
                commandType: CommandType.Text);
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
            apprenticeshipsTable.Columns.Add("ReservationId", typeof(Guid));
            apprenticeshipsTable.Columns.Add("Email", typeof(string));
            return apprenticeshipsTable;
        }

        private DataRow AddApprenticeshipToTable(DataTable apprenticeshipsTable, long commitmentId, Apprenticeship a)
        {
            return apprenticeshipsTable.Rows.Add(commitmentId, a.FirstName, a.LastName, a.ULN, a.TrainingType, a.TrainingCode, a.TrainingName,
                a.Cost, a.StartDate, a.EndDate, a.AgreementStatus, a.PaymentStatus, a.DateOfBirth, a.NINumber,
                a.EmployerRef, a.ProviderRef, _currentDateTime.Now, a.ReservationId, a.Email);
        }

        private static async Task<Commitment> GetCommitment(long commitmentId, IDbConnection connection, IDbTransaction transation = null)
        {
            var lookup = new Dictionary<object, Commitment>();
            var mapper = new ParentChildrenMapper<Commitment, Apprenticeship>();

            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", commitmentId);

            var results = await connection.QueryAsync(
                sql: "[dbo].[GetCommitment]",
                param: parameters,
                transaction: transation,
                commandType: CommandType.StoredProcedure,
                map: mapper.Map(lookup, x => x.Id, x => x.Apprenticeships));

            return lookup.Values.SingleOrDefault();
        }

        private Task<ApprenticeshipsResult> GetApprenticeshipsByIdentifier(string identifierName, long identifierValue, string searchKeyword)
        {
            return WithConnection(async c =>
                {
                    var s = searchKeyword?.Trim() ?? string.Empty;
                    var parameters = new DynamicParameters();
                    parameters.Add("@now", _currentDateTime.Now);
                    parameters.Add(identifierName, identifierValue, DbType.Int64);

                    const string sql = "[GetApprenticeshipsWithPriceHistory]";

                    var apprenticeships = new Dictionary<long, Apprenticeship>();
                    int count;
                    using (var multi = await c.QueryMultipleAsync(sql, parameters, commandType: CommandType.StoredProcedure))
                    {
                        multi.Read<Apprenticeship, PriceHistory, Apprenticeship>(
                            (apprenticeship, history) =>
                                {
                                    Apprenticeship existing;
                                    if (!apprenticeships.TryGetValue(apprenticeship.Id, out existing))
                                    {
                                        apprenticeships.Add(apprenticeship.Id, apprenticeship);
                                        existing = apprenticeship;
                                    }

                                    if (history.ApprenticeshipId != 0)
                                    {
                                        existing.PriceHistory.Add(history);
                                    }

                                    return existing;
                                });
                        count = multi.Read<int>().First();
                    }

                    return new ApprenticeshipsResult
                    {
                        Apprenticeships = apprenticeships.Values.ToList(),
                        TotalCount = count
                    };

                });
        }

        public async Task<IEnumerable<ApprenticeshipResult>> GetActiveApprenticeshipsByUlns(IEnumerable<string> ulns)
        {
            var ulnDataTable = GenerateUlnDataTable(ulns);

            return await WithConnection(async c => await c.QueryAsync<ApprenticeshipResult>(
                "[dbo].[GetActiveApprenticeshipsByULNs]",
                new { ULNs = ulnDataTable.AsTableValuedParameter("dbo.ULNTable") },
                commandType: CommandType.StoredProcedure));
        }

        public async Task<IEnumerable<ApprenticeshipStatusSummary>> GetApprenticeshipSummariesByEmployer(long employerAccountId)
        {
            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@employerAccountId", employerAccountId);

                var results = await connection.QueryAsync(
                    sql: "[dbo].[GetApprenticeshipStatusSummaries]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return MapToApprenticeshipStatusSummaries(results);
            });
        }

        private static IEnumerable<ApprenticeshipStatusSummary> MapToApprenticeshipStatusSummaries(IEnumerable<dynamic> results)
        {
            var apprenticeshipsStatusSummaries = new Dictionary<string, ApprenticeshipStatusSummary>();

            foreach (var result in results.ToList())
            {
                var legalEntityId = (string) result.LegalEntityId;
                var organisationType = (SFA.DAS.Common.Domain.Types.OrganisationType) result.LegalEntityOrganisationType;
                var paymentStatus = (PaymentStatus) result.PaymentStatus;
                var count = (int) result.Count;

                if (!apprenticeshipsStatusSummaries.ContainsKey(legalEntityId))
                {
                    apprenticeshipsStatusSummaries.Add(legalEntityId, new ApprenticeshipStatusSummary
                    {
                        LegalEntityIdentifier = legalEntityId,
                        LegalEntityOrganisationType = organisationType
                    });
                }

                var apprenticeshipStatusSummary = apprenticeshipsStatusSummaries[legalEntityId];

                switch (paymentStatus)
                {
                    case PaymentStatus.PendingApproval:
                        apprenticeshipStatusSummary.PendingApprovalCount = count;
                        break;
                    case PaymentStatus.Active:
                        apprenticeshipStatusSummary.ActiveCount = count;
                        break;
                    case PaymentStatus.Paused:
                        apprenticeshipStatusSummary.PausedCount = count;
                        break;
                    case PaymentStatus.Withdrawn:
                        apprenticeshipStatusSummary.WithdrawnCount = count;
                        break;
                    case PaymentStatus.Completed:
                        apprenticeshipStatusSummary.CompletedCount = count;
                        break;
                    case PaymentStatus.Deleted:
                    default:
                        throw new ArgumentOutOfRangeException($"Unexpected payment status '{paymentStatus}' found when creating apprenticeship summary statuses");
                }
            }

            return apprenticeshipsStatusSummaries.Values;
        }

        private static DataTable GenerateUlnDataTable(IEnumerable<string> ulns)
        {
            var result = new DataTable();

            result.Columns.Add("ULN", typeof(string));

            foreach (var uln in ulns.Where(u => !string.IsNullOrWhiteSpace(u) && u.Length <= 50))
            {
                result.Rows.Add(uln);
            }

            return result;
        }


        public async Task<ApprenticeshipsResult> GetApprovedApprenticeshipsByEmployer(long accountId)
        {
            return await GetApprovedApprenticeships("GetApprovedApprenticeshipsForEmployer", accountId);
        }

        public async Task<ApprenticeshipsResult> GetApprovedApprenticeshipsByProvider(long accountId)
        {
            return await GetApprovedApprenticeships("GetApprovedApprenticeshipsForProvider", accountId);
        }

        private Task<ApprenticeshipsResult> GetApprovedApprenticeships(string sprocName, long id)
        {
            return WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", id, DbType.Int64);

                var apprenticeships = new Dictionary<long, Apprenticeship>();

                using (var multi = await c.QueryMultipleAsync(sprocName, parameters, commandType: CommandType.StoredProcedure))
                {
                    multi.Read<Apprenticeship, DataLockStatusSummary, Apprenticeship>(
                        (apprenticeship, datalock) =>
                        {
                            if (!apprenticeships.TryGetValue(apprenticeship.Id, out var existing))
                            {
                                apprenticeships.Add(apprenticeship.Id, apprenticeship);
                                existing = apprenticeship;
                            }

                            if (datalock != null)
                            {
                                existing.DataLocks.Add(datalock);
                            }

                            return existing;
                        },
                        splitOn: "DataLockEventId");
                }

                return new ApprenticeshipsResult
                {
                    Apprenticeships = apprenticeships.Values.ToList(),
                    TotalCount = apprenticeships.Values.Count()
                };
            });
        }

        public async Task<IEnumerable<OverlappingEmail>> GetEmaiOverlaps(List<EmailToValidate> emailToValidate)
        {
            var emailDataTable = BuildEmailCheckTable(emailToValidate);

            return await WithConnection(async c => await c.QueryAsync<OverlappingEmail>(
                "[dbo].[CheckForOverlappingEmailsForTable]",
                new { Emails = emailDataTable.AsTableValuedParameter("dbo.EmailCheckTable") },
                commandType: CommandType.StoredProcedure));
        }

        private DataTable BuildEmailCheckTable(List<EmailToValidate> emailToValidate)
        {
            var apprenticeshipsTable = CreateEmailCheckTable();

            foreach (var apprenticeship in emailToValidate)
            {
                AddEmailToValidateToTable(apprenticeshipsTable, apprenticeship);
            }

            return apprenticeshipsTable;
        }


        private static DataTable CreateEmailCheckTable()
        {
            var apprenticeshipsTable = new DataTable();

            apprenticeshipsTable.Columns.Add("RowId", typeof(long));
            apprenticeshipsTable.Columns.Add("Email", typeof(string));
            apprenticeshipsTable.Columns.Add("StartDate", typeof(DateTime));
            apprenticeshipsTable.Columns.Add("EndDate", typeof(DateTime));
            apprenticeshipsTable.Columns.Add("ApprenticeshipId", typeof(long));

            return apprenticeshipsTable;
        }

        private DataRow AddEmailToValidateToTable(DataTable apprenticeshipsTable, EmailToValidate a)
        {
            return apprenticeshipsTable.Rows.Add(a.RowId, a.Email, a.StartDate, a.EndDate, a.ApprenticeshipId);
        }

    }
}
