using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class StatisticsRepository : BaseRepository, IStatisticsRepository
    {
        private readonly ICommitmentsLogger _logger;

        public StatisticsRepository(string connectionString, ICommitmentsLogger logger) : base(connectionString, logger.BaseLogger)
        {
            _logger = logger;
        }

        public Task<Statistics> GetStatistics()
        {
            return WithConnection(async c =>
            {
                _logger.Info("Getting statistics for consistency checks with RDS");

                var results = await c.QueryAsync<Statistics>(sql: $"[dbo].[GetStatistics]",
                    commandType: CommandType.StoredProcedure);

                return results.FirstOrDefault();
            });
        }
    }
}
