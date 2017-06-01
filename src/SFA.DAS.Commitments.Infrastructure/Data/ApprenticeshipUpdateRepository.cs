using System;
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
            IApprenticeshipTransactions apprenticeshipTransactions) : base(connectionString, logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if(apprenticeshipUpdateTransactions==null)
                throw new ArgumentNullException(nameof(apprenticeshipUpdateTransactions));
            if (apprenticeshipTransactions == null)
                throw new ArgumentNullException(nameof(apprenticeshipTransactions));

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
                    await _apprenticeshipUpdateTransactions.CreateApprenticeshipUpdate(connection, trans,
                        apprenticeshipUpdate);
                }

                if (apprenticeship != null)
                {
                    await _apprenticeshipUpdateTransactions.UpdateApprenticeshipReferenceAndUln(connection, trans,
                        apprenticeship);
                }

                return 0;
            });
        }

        public async Task ApproveApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, string userId, Apprenticeship apprenticeship, Caller caller)
        {
            await WithTransaction(async (connection, trans) =>
            {
                await _apprenticeshipTransactions.UpdateApprenticeship(connection, trans, apprenticeship, caller);

                await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdate.Id, userId, ApprenticeshipUpdateStatus.Approved);

                if (apprenticeshipUpdate.UpdateOrigin == UpdateOrigin.DataLock)
                {
                    await ResolveDatalock(connection, trans, apprenticeshipUpdate.Id);
                }

                return 1L;
            });
        }

        public async Task RejectApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, string userId)
        {
            await WithTransaction(async (connection, trans) =>
                {
                    await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdate.Id, userId,
                        ApprenticeshipUpdateStatus.Rejected);

                    if (apprenticeshipUpdate.UpdateOrigin == UpdateOrigin.DataLock)
                    {
                        await ResetDatalockTriage(connection, trans, apprenticeshipUpdate.Id);
                    }

                    return 1L;
                });
        }

        public async Task UndoApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, string userId)
        {
            await WithTransaction(async (connection, trans) =>
            {
                await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdate.Id, userId,
                    ApprenticeshipUpdateStatus.Deleted);

                if (apprenticeshipUpdate.UpdateOrigin == UpdateOrigin.DataLock)
                {
                    await ResetDatalockTriage(connection, trans, apprenticeshipUpdate.Id);
                }

                return 1L;
            });
        }

        private async Task UpdateApprenticeshipUpdate(IDbConnection connection, IDbTransaction trans, long apprenticeshipUpdateId, string userId, ApprenticeshipUpdateStatus updateStatus)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@id", apprenticeshipUpdateId, DbType.Int64);
            parameters.Add("@status", updateStatus, DbType.Int16);

            await connection.ExecuteAsync(
                    sql:
                    "UPDATE [dbo].[ApprenticeshipUpdate] SET Status = @status " +
                    "WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text,
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

        public async Task SupercedeApprenticeshipUpdate(long apprenticeshipUpdateId)
        {
            await WithTransaction(async (connection, trans) =>
            {
                await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdateId, string.Empty,
                    ApprenticeshipUpdateStatus.Superceded);
                return 1L;
            });
        }
    }
}
