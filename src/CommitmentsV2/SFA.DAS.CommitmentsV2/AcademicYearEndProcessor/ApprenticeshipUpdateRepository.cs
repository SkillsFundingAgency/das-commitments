using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure.Data.Transactions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data
{
    public class ApprenticeshipUpdateRepository : BaseRepository, IApprenticeshipUpdateRepository
    {
        private readonly ILogger<ApprenticeshipUpdateRepository> _logger;
        private readonly IApprenticeshipUpdateTransactions _apprenticeshipUpdateTransactions;
        private readonly IApprenticeshipTransactions _apprenticeshipTransactions;

        public ApprenticeshipUpdateRepository(
            string connectionString,
            ILogger<ApprenticeshipUpdateRepository> logger,
            IApprenticeshipUpdateTransactions apprenticeshipUpdateTransactions,
            IApprenticeshipTransactions apprenticeshipTransactions) : base(connectionString, logger)
        {
            _logger = logger;
            _apprenticeshipUpdateTransactions = apprenticeshipUpdateTransactions;
            _apprenticeshipTransactions = apprenticeshipTransactions;

        }

        public async Task<ApprenticeshipUpdate_new> GetPendingApprenticeshipUpdate(long apprenticeshipId)
        {
            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@apprenticeshipId", apprenticeshipId);

                var results = await connection.QueryAsync<ApprenticeshipUpdate_new>(
                    sql: $"[dbo].[GetApprenticeshipUpdate]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return results.SingleOrDefault();
            });
        }

        public async Task CreateApprenticeshipUpdate(ApprenticeshipUpdate_new apprenticeshipUpdate, Apprenticeship_new apprenticeship)
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

        public async Task ApproveApprenticeshipUpdate(ApprenticeshipUpdate_new apprenticeshipUpdate, Apprenticeship_new apprenticeship, Caller caller)
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

        public async Task RejectApprenticeshipUpdate(ApprenticeshipUpdate_new apprenticeshipUpdate)
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

        public async Task UndoApprenticeshipUpdate(ApprenticeshipUpdate_new apprenticeshipUpdate)
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

        public async Task<IEnumerable<ApprenticeshipUpdate_new>> GetExpiredApprenticeshipUpdates(DateTime currentAcademicYearStartDate)
        {
            _logger.LogInformation("Getting all expired apprenticeship update");

            var parameters = new DynamicParameters();
            parameters.Add("@status", ApprenticeshipUpdateStatus.Pending, DbType.Int16);
            parameters.Add("@date", currentAcademicYearStartDate, DbType.DateTime);

            return await WithTransaction(
                async (connection, trans) => await
                    connection.QueryAsync<ApprenticeshipUpdate_new>(
                        sql: $"[dbo].[GetApprenticeshipUpdatesByDateAndStatus]",
                        param: parameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: trans));
        }

        public async Task ExpireApprenticeshipUpdate(long apprenticeshipUpdateId)
        {
            _logger.LogInformation($"Updating apprenticeship update {apprenticeshipUpdateId} - to expired");

            await WithConnection(async connection =>
            {
                await UpdateApprenticeshipUpdate(connection, null, apprenticeshipUpdateId, ApprenticeshipUpdateStatus.Expired);
                return 1L;
            });
        }
    }
}
