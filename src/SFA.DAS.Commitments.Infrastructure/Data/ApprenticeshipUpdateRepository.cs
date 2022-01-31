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
using SFA.DAS.Commitments.Infrastructure.Data.Transactions;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class ApprenticeshipUpdateRepository : BaseRepository, IApprenticeshipUpdateRepository
    {
        private readonly ICommitmentsLogger _logger;
        private readonly IApprenticeshipUpdateTransactions _apprenticeshipUpdateTransactions;
        private readonly IApprenticeshipTransactions _apprenticeshipTransactions;

        public ApprenticeshipUpdateRepository(
            string connectionString, 
            ICommitmentsLogger logger, 
            IApprenticeshipUpdateTransactions apprenticeshipUpdateTransactions,
            IApprenticeshipTransactions apprenticeshipTransactions) : base(connectionString, logger.BaseLogger)
        {
            _logger = logger;
            _apprenticeshipUpdateTransactions = apprenticeshipUpdateTransactions;
            _apprenticeshipTransactions = apprenticeshipTransactions;
        }

        public async Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long apprenticeshipId)
        {
            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@apprenticeshipId", apprenticeshipId);

                var results = await connection.QueryAsync<ApprenticeshipUpdate>(
                    sql: $"[dbo].[GetApprenticeshipUpdate]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return results.SingleOrDefault();
            });
        }

        public async Task CreateApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, Apprenticeship apprenticeship)
        {
            await WithTransaction(async (connection, trans) =>
            {
                if (apprenticeshipUpdate != null)
                {
                    // this also sets PendingUpdateOriginator to null on apprenticeship
                    await _apprenticeshipUpdateTransactions.CreateApprenticeshipUpdate(connection, trans, apprenticeshipUpdate);
                }

                if (apprenticeship != null)
                {
                    await _apprenticeshipUpdateTransactions.UpdateApprenticeshipReferenceAndUln(connection, trans, apprenticeship);
                }

                return 0;
            });
        }

        public async Task ApproveApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, Apprenticeship apprenticeship, Caller caller)
        {
            await WithTransaction(async (connection, trans) =>
            {
                await _apprenticeshipTransactions.UpdateApprenticeship(connection, trans, apprenticeship, caller);

                await _apprenticeshipTransactions.UpdateCurrentPrice(connection, trans, apprenticeship);

                // this also sets PendingUpdateOriginator to null in apprenticeship
                await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdate.Id, ApprenticeshipUpdateStatus.Approved);

                if (apprenticeshipUpdate.UpdateOrigin == UpdateOrigin.DataLock)
                {
                    await ResolveDatalock(connection, trans, apprenticeshipUpdate.Id);
                }

                return 1L;
            });
        }

        public async Task RejectApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate)
        {
            await WithTransaction(async (connection, trans) =>
                {
                    // this also sets PendingUpdateOriginator to null in apprenticeship
                    await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdate.Id,
                        ApprenticeshipUpdateStatus.Rejected);

                    if (apprenticeshipUpdate.UpdateOrigin == UpdateOrigin.DataLock)
                    {
                        await ResetDatalockTriage(connection, trans, apprenticeshipUpdate.Id);
                    }

                    return 1L;
                });
        }

        public async Task UndoApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate)
        {
            await WithTransaction(async (connection, trans) =>
            {
                // this also sets PendingUpdateOriginator to null in apprenticeship
                await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdate.Id,
                    ApprenticeshipUpdateStatus.Deleted);

                if (apprenticeshipUpdate.UpdateOrigin == UpdateOrigin.DataLock)
                {
                    await ResetDatalockTriage(connection, trans, apprenticeshipUpdate.Id);
                }

                return 1L;
            });
        }

        private async Task UpdateApprenticeshipUpdate(IDbConnection connection, IDbTransaction trans, long apprenticeshipUpdateId, ApprenticeshipUpdateStatus updateStatus)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@id", apprenticeshipUpdateId, DbType.Int64);
            parameters.Add("@status", updateStatus, DbType.Int16);

            await connection.ExecuteAsync(
                    sql: "[UpdateApprenticeshipUpdateStatus]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: trans);
        }

        private async Task ResolveDatalock(IDbConnection connection, IDbTransaction trans, long apprenticeshipUpdateId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ApprenticeshipUpdateId", apprenticeshipUpdateId, DbType.Int64);

            await connection.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[DataLockStatus] SET IsResolved=1 " +
                    "WHERE ApprenticeshipUpdateId = @apprenticeshipUpdateId;",
                    param: parameters,
                    commandType: CommandType.Text,
                    transaction: trans);
        }

        private async Task ResetDatalockTriage(IDbConnection connection, IDbTransaction trans, long apprenticeshipUpdateId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ApprenticeshipUpdateId", apprenticeshipUpdateId, DbType.Int64);

            await connection.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[DataLockStatus] SET TriageStatus=0, ApprenticeshipUpdateId=null " +
                    "WHERE ApprenticeshipUpdateId = @apprenticeshipUpdateId;",
                    param: parameters,
                    commandType: CommandType.Text,
                    transaction: trans);
        }

        public async Task<IEnumerable<ApprenticeshipUpdate>> GetExpiredApprenticeshipUpdates(DateTime currentAcademicYearStartDate)
        {
            _logger.Info("Getting all expired apprenticeship update");

            var parameters = new DynamicParameters();
            parameters.Add("@status", ApprenticeshipUpdateStatus.Pending, DbType.Int16);
            parameters.Add("@date", currentAcademicYearStartDate, DbType.DateTime);

            return await WithTransaction(
                async (connection, trans) => await
                    connection.QueryAsync<ApprenticeshipUpdate>(
                        sql: $"[dbo].[GetApprenticeshipUpdatesByDateAndStatus]",
                        param: parameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: trans));
        }

        public async Task ExpireApprenticeshipUpdate(long apprenticeshipUpdateId)
        {
            _logger.Info($"Updating apprenticeship update {apprenticeshipUpdateId} - to expired");

            await WithConnection(async connection =>
            {
                await UpdateApprenticeshipUpdate(connection, null, apprenticeshipUpdateId, ApprenticeshipUpdateStatus.Expired);
                return 1L;
            });
        }
    }
}
