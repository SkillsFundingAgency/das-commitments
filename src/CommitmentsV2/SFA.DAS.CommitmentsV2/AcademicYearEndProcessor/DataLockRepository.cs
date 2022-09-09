using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities.DataLock;
using System.Data.SqlClient;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using System;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data
{
    public class DataLockRepository : BaseRepository, IDataLockRepository
    {
        private readonly ILogger<DataLockRepository> _logger;

        public DataLockRepository(string connectionString, ILogger<DataLockRepository> logger
            ) : base(connectionString, null)
        {
            _logger = logger;
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
            catch (Exception ex) when (ex.InnerException is SqlException)
            {
                throw new RepositoryConstraintException("Unable to update datalockstatus record to expire record", ex);
            }
        }
    }
}
