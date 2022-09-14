using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Data;
using System.Data.SqlClient;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data
{
    public class DataLockRepository : IDataLockRepository
    {
        private readonly ILogger<DataLockRepository> _logger;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly ProviderCommitmentsDbContext _dbContext;
        public DataLockRepository(string connectionString,
            ILogger<DataLockRepository> logger,
            ProviderCommitmentsDbContext dbContext,
            ICurrentDateTime currentDateTime
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _currentDateTime = currentDateTime;
        }
        public async Task<List<DataLockStatus>> GetExpirableDataLocks(DateTime beforeDate)
        {
            _logger.LogInformation("Getting Alert Summaries for employer accounts");

            var query = _dbContext.DataLocks
                .Where(app => app.IsExpired == false)
                .Where(app => app.IlrEffectiveFromDate < beforeDate);

            List<DataLockStatus> results = await query.ToListAsync();

            if (results.Any())
            {
                _logger.LogInformation("Retrieved Alert Summaries for employer accounts");
            }
            else
            {
                _logger.LogInformation($"Cannot find any Alert Summaries for employer accounts");
            }

            return results;
        }

        public async Task<int> UpdateExpirableDataLocks(DataLockStatus dataLockStatus)
        {
            try
            {
                dataLockStatus.IsExpired = true;
                dataLockStatus.Expired = _currentDateTime.UtcNow;

                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex) when (ex.InnerException is SqlException)
            {
                throw new RepositoryConstraintException("Unable to update datalockstatus record to expire record", ex);
            }
        }
    }
}
