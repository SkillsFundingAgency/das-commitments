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
            IApprenticeshipTransactions apprenticeshipTransactions) : base(connectionString)
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

        public async Task ApproveApprenticeshipUpdate(long id, string userId, Apprenticeship apprenticeship, Caller caller)
        {
            await WithTransaction(async (connection, trans) =>
            {
                await _apprenticeshipTransactions.UpdateApprenticeship(connection, trans, apprenticeship, caller);

                await UpdateApprenticeshipUpdate(connection, trans, id, userId, ApprenticeshipUpdateStatus.Approved);

                return 1L;
            });
        }

        public async Task RejectApprenticeshipUpdate(long apprenticeshipUpdateId, string userId)
        {
            await WithTransaction(async (connection, trans) =>
                {
                    await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdateId, userId,
                        ApprenticeshipUpdateStatus.Rejected);
                    return 1L;
                });
        }

        public async Task UndoApprenticeshipUpdate(long apprenticeshipUpdateId, string userId)
        {
            await WithTransaction(async (connection, trans) =>
            {
                await UpdateApprenticeshipUpdate(connection, trans, apprenticeshipUpdateId, userId,
                    ApprenticeshipUpdateStatus.Deleted);
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
    }
}
