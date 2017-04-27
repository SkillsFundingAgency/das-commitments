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
    public class ApprenticeshipRepository : BaseRepository, IApprenticeshipRepository
    {
        private readonly ICommitmentsLogger _logger;

        private readonly IHistoryTransactions _historyTransactions;

        private readonly IApprenticeshipTransactions _apprenticeshipTransactions;

        public ApprenticeshipRepository(
            string connectionString,
            ICommitmentsLogger logger,
            IHistoryTransactions historyTransactions,
            IApprenticeshipTransactions apprenticeshipTransactions)
            : base(connectionString)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (historyTransactions == null)
                throw new ArgumentNullException(nameof(historyTransactions));
            if (apprenticeshipTransactions == null)
                throw new ArgumentNullException(nameof(apprenticeshipTransactions));

            _logger = logger;
            _historyTransactions = historyTransactions;
            _apprenticeshipTransactions = apprenticeshipTransactions;
        }

        public async Task<long> CreateApprenticeship(Apprenticeship apprenticeship, CallerType callerType, string userId)
        {
            _logger.Debug($"Creating apprenticeship - {apprenticeship.FirstName} {apprenticeship.LastName}", accountId: apprenticeship.EmployerAccountId, providerId: apprenticeship.ProviderId, commitmentId: apprenticeship.CommitmentId);

            return await WithTransaction(async (connection, trans)=>
                {
                    var apprenticeshipId = await _apprenticeshipTransactions.CreateApprenticeship(connection, trans, apprenticeship);
                    await _historyTransactions.CreateApprenticeship(connection, trans,
                        new ApprenticeshipHistoryItem
                        {
                            ApprenticeshipId = apprenticeshipId,
                            UpdatedByRole = callerType,
                            UserId = userId
                        });

                    await _historyTransactions.AddApprenticeshipForCommitment(connection, trans, 
                        new CommitmentHistoryItem
                        {
                            CommitmentId = apprenticeship.CommitmentId, 
                            UpdatedByRole = callerType,
                            UserId = userId
                        });

                    return apprenticeshipId;
                });
        }

        public async Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller, string userId)
        {
            _logger.Debug($"Updating apprenticeship {apprenticeship.Id}", accountId: apprenticeship.EmployerAccountId, providerId: apprenticeship.ProviderId, commitmentId: apprenticeship.CommitmentId, apprenticeshipId: apprenticeship.Id);

            await WithTransaction(async (connection, trans) =>
                {
                    var returnCode = await _apprenticeshipTransactions.UpdateApprenticeship(connection, trans, apprenticeship, caller);

                    await _historyTransactions.UpdateApprenticeship(connection, trans, 
                        new ApprenticeshipHistoryItem
                        {
                            ApprenticeshipId = apprenticeship.Id,
                            UpdatedByRole = caller.CallerType,
                            UserId = userId
                        });

                    await _historyTransactions.UpdateApprenticeshipForCommitment(connection, trans,
                        new CommitmentHistoryItem
                    {
                        CommitmentId = apprenticeship.CommitmentId,
                        UpdatedByRole = caller.CallerType,
                        UserId = userId
                    });

                    return returnCode;
            });
        }

        public async Task StopApprenticeship(long commitmentId, long apprenticeshipId, DateTime dateOfChange, CallerType callerType, string userId)
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

                await _historyTransactions.UpdateApprenticeshipStatus(conn, tran, PaymentStatus.Withdrawn,
                    new ApprenticeshipHistoryItem
                    {
                        ApprenticeshipId = apprenticeshipId,
                        UpdatedByRole = callerType,
                        UserId = userId
                    });
            });
        }

        public async Task PauseOrResumeApprenticeship(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus, DateTime dateOfChange, CallerType callerType, string userId)
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

                await _historyTransactions.UpdateApprenticeshipStatus(conn, tran, paymentStatus,
                    new ApprenticeshipHistoryItem
                    {
                        ApprenticeshipId = apprenticeshipId,
                        UpdatedByRole = callerType,
                        UserId = userId
                    });
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

        public async Task DeleteApprenticeship(long apprenticeshipId, CallerType callerType, string userId, long commitmentId)
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

                await _historyTransactions.DeleteApprenticeshipForCommitment(connection, transactions, 
                    new CommitmentHistoryItem
                        {
                            CommitmentId = commitmentId,
                            UpdatedByRole = CallerType.Employer,
                            UserId = userId
                        });

                return returnCode;
            });
        }

        public async Task<IList<Apprenticeship>> BulkUploadApprenticeships(long commitmentId, IEnumerable<Apprenticeship> apprenticeships, CallerType caller, string userId)
        {
            _logger.Debug($"Bulk upload {apprenticeships.Count()} apprenticeships for commitment {commitmentId}", commitmentId: commitmentId);

            var table = BuildApprenticeshipDataTable(apprenticeships);

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

        private static async Task<IList<Apprenticeship>> UploadApprenticeshipsAndGetIds(long commitmentId, IDbConnection x, DataTable table)
        {
            IList<Apprenticeship> apprenticeships;

            using (var tran = x.BeginTransaction())
            {
                await x.ExecuteAsync(
                    sql: "[dbo].[BulkUploadApprenticships]",
                    transaction: tran,
                    commandType: CommandType.StoredProcedure,
                    param: new { @commitmentId = commitmentId, @apprenticeships = table.AsTableValuedParameter("dbo.ApprenticeshipTable") }
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

        private static DataRow AddApprenticeshipToTable(DataTable apprenticeshipsTable, Apprenticeship a)
        {
            return apprenticeshipsTable.Rows.Add(a.FirstName, a.LastName, a.ULN, a.TrainingType, a.TrainingCode, a.TrainingName,
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

            foreach (var uln in ulns)
            {
                result.Rows.Add(uln);
            }

            return result;
        }
    }
}
