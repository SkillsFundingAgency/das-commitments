using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Infrastructure.Data.Transactions;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Domain.Exceptions;
using System;
using System.Text;
using Microsoft.Data.SqlClient;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class DataLockRepository : BaseRepository, IDataLockRepository
    {
        private readonly IDataLockTransactions _dataLockTransactions;
        private readonly ICommitmentsLogger _logger;

        public DataLockRepository(string connectionString,
            IDataLockTransactions dataLockTransactions,
            ICommitmentsLogger logger) : base(connectionString, logger.BaseLogger)
        {
            _dataLockTransactions = dataLockTransactions;
            _logger = logger;
        }

        public async Task<long> GetLastDataLockEventId()
        {
            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                var results = await connection.QueryAsync<long?>(
                    sql: $"[dbo].[GetLastDataLockEventId]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
                var result = results.Single();
                return result ?? 0;
            });
        }

        public async Task<long> UpdateDataLockStatus(DataLockStatus dataLockStatus)
        {
            _logger.Info($"Updating or inserting data lock status {dataLockStatus.DataLockEventId}, EventsStatus: {dataLockStatus.EventStatus}");
            try
            {
                var result = await WithConnection(async connection =>
                {
                    var parameters = new DynamicParameters();

                    parameters.Add("@DataLockEventId", dataLockStatus.DataLockEventId);
                    parameters.Add("@DataLockEventDatetime", dataLockStatus.DataLockEventDatetime);
                    parameters.Add("@PriceEpisodeIdentifier", dataLockStatus.PriceEpisodeIdentifier);
                    parameters.Add("@ApprenticeshipId", dataLockStatus.ApprenticeshipId);
                    parameters.Add("@IlrTrainingCourseCode", dataLockStatus.IlrTrainingCourseCode);
                    parameters.Add("@IlrTrainingType", dataLockStatus.IlrTrainingType);
                    parameters.Add("@IlrActualStartDate", dataLockStatus.IlrActualStartDate);
                    parameters.Add("@IlrEffectiveFromDate", dataLockStatus.IlrEffectiveFromDate);
                    parameters.Add("@IlrPriceEffectiveToDate", dataLockStatus.IlrPriceEffectiveToDate);
                    parameters.Add("@IlrTotalCost", dataLockStatus.IlrTotalCost);
                    parameters.Add("@ErrorCode", dataLockStatus.ErrorCode);
                    parameters.Add("@Status", dataLockStatus.Status);
                    parameters.Add("@TriageStatus", dataLockStatus.TriageStatus);
                    parameters.Add("@ApprenticeshipUpdateId", dataLockStatus.ApprenticeshipUpdateId);
                    parameters.Add("@IsResolved", dataLockStatus.IsResolved);
                    parameters.Add("@EventStatus", dataLockStatus.EventStatus);

                    return await connection.ExecuteAsync(
                        sql: $"[dbo].[UpdateDataLockStatus]",
                        param: parameters,
                        commandType: CommandType.StoredProcedure);
                });

                return result;
            }
            catch (Exception ex) when (ex.InnerException is SqlException && IsConstraintError(ex.InnerException as SqlException))
            {
                throw new RepositoryConstraintException("Unable to insert datalockstatus record", ex);
            }
        }

        private static bool IsConstraintError(SqlException ex)
        {
            return ex.Errors?.Count == 2 && ex.Errors[0].Number == 547;
        }

        public async Task<List<DataLockStatus>> GetDataLocks(long apprenticeshipId, bool includeRemoved)
        {
            return await WithConnection(async connection =>
            {
                var sql = new StringBuilder();
                sql.Append($"SELECT * FROM DataLockStatus WHERE ApprenticeshipId = @ApprenticeshipId ");
                if (!includeRemoved) sql.Append("AND EventStatus <> 3 AND IsExpired = 0 ");
                sql.Append("ORDER BY IlrEffectiveFromDate, Id");

                var parameters = new DynamicParameters();
                parameters.Add("@ApprenticeshipId", apprenticeshipId);
                var results = await connection.QueryAsync<DataLockStatus>(
                    sql: sql.ToString(),
                    param: parameters,
                    commandType: CommandType.Text);
                return results.ToList();
            });
        }

        public async Task<DataLockStatus> GetDataLock(long dataLockEventId)
        {
            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@DataLockEventId", dataLockEventId);
                var results = await connection.QueryAsync<DataLockStatus>(
                    sql: $"[dbo].[GetDataLockStatus]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
                return results.SingleOrDefault();
            });
        }

        public async Task UpdateDataLockTriageStatus(long dataLockEventId, TriageStatus triageStatus)
        {
            await WithTransaction(async (connection, trans) =>
            {
                await _dataLockTransactions.UpdateDataLockTriageStatus(connection, trans, dataLockEventId, triageStatus);
            });
        }

        public async Task UpdateDataLockTriageStatus(IEnumerable<long> dataLockEventIds, TriageStatus triageStatus)
        {
            await WithTransaction(async (connection, trans) =>
            {
                foreach (var id in dataLockEventIds)
                {    
                    await _dataLockTransactions.UpdateDataLockTriageStatus(connection, trans, id, triageStatus);
                }
            });
        }

        public async Task ResolveDataLock(IEnumerable<long> dataLockEventIds)
        {
            await WithTransaction(async (connection, trans) =>
            {
                foreach (var id in dataLockEventIds)
                {
                    await _dataLockTransactions.ResolveDataLock(connection, trans, id);
                }
            });
        }

        public async Task Delete(long dataLockEventId)
        {
            await WithConnection(async (connection) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@DataLockEventId", dataLockEventId);

                return await connection.ExecuteAsync(
                    sql: "DELETE [dbo].[DataLockStatus] "
                       + "WHERE DataLockEventId = @DataLockEventId",
                    param: parameters,
                    commandType: CommandType.Text);
            });
        }

        public async Task<List<DataLockStatus>> GetExpirableDataLocks(DateTime beforeDate)
        {
            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@BeforeDate", beforeDate);
                 var results = await connection.QueryAsync<DataLockStatus>(
                    sql: $"[dbo].[GetDataLockStatusExpiryCandidates]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
                return results.ToList();
            });
        }

        public async Task<bool> UpdateExpirableDataLocks(long apprenticeshipId, string priceEpisodeIdentifier, DateTime expiredDateTime)
        {
            try
            {
                var result = await WithConnection(async connection =>
                {
                    var parameters = new DynamicParameters();

                    parameters.Add("@ApprenticeshipId", apprenticeshipId);
                    parameters.Add("@PriceEpisodeIdentifier", priceEpisodeIdentifier);
                    parameters.Add("@ExpiredDateTime", expiredDateTime);


                    return await connection.ExecuteAsync(
                        sql: $"[dbo].[UpdateDatalockStatusIsExpired]",
                        param: parameters,
                        commandType: CommandType.StoredProcedure);
                });

                return result == 0;
            }
            catch (Exception ex) when (ex.InnerException is SqlException )
            {
                throw new RepositoryConstraintException("Unable to update datalockstatus record to expire record", ex);
            }
        }
    }
}
