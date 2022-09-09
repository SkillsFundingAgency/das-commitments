using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data
{
    public class ApprenticeshipUpdateRepository : BaseRepository, IApprenticeshipUpdateRepository
    {
        private readonly ILogger<ApprenticeshipUpdateRepository> _logger;
        public ApprenticeshipUpdateRepository(
            string connectionString,
            ILogger<ApprenticeshipUpdateRepository> logger
           ) : base(connectionString, logger)
        {
            _logger = logger;
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
        public async Task<IEnumerable<ApprenticeshipUpdateDetails>> GetExpiredApprenticeshipUpdates(DateTime currentAcademicYearStartDate)
        {
            _logger.LogInformation("Getting all expired apprenticeship update");

            var parameters = new DynamicParameters();
            parameters.Add("@status", ApprenticeshipUpdateStatus.Pending, DbType.Int16);
            parameters.Add("@date", currentAcademicYearStartDate, DbType.DateTime);

            return await WithTransaction(
                async (connection, trans) => await
                    connection.QueryAsync<ApprenticeshipUpdateDetails>(
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
