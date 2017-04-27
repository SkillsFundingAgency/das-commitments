using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class DataLockRepository : BaseRepository, IDataLockRepository
    {
        public DataLockRepository(string connectionString) : base(connectionString)
        {
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
            return await WithConnection(async connection =>
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
                parameters.Add("@IlrTotalCost", dataLockStatus.IlrTotalCost);
                parameters.Add("@ErrorCodes", dataLockStatus.ErrorCode);
                parameters.Add("@Status", dataLockStatus.Status);
                parameters.Add("@TriageStatus", dataLockStatus.TriageStatus);

                return await connection.ExecuteAsync(
                    sql: $"[dbo].[UpdateDataLockStatus]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });
        }

        public Task<List<DataLockStatus>> GetDataLocks(long apprenticeshipId)
        {
            throw new NotImplementedException();
        }

        public Task<DataLockStatus> GetDataLock(long dataLockEventId)
        {
            throw new NotImplementedException();
        }
    }
}
