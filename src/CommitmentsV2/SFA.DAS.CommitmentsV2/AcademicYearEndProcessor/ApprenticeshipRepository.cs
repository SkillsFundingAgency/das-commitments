using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data
{
    public class ApprenticeshipRepository : BaseRepository, IApprenticeshipRepository
    {
        private readonly ILogger<ApprenticeshipRepository> _logger;
        private readonly ICurrentDateTime _currentDateTime;

        public ApprenticeshipRepository(string connectionString,
            ILogger<ApprenticeshipRepository> logger,
            ICurrentDateTime currentDateTime) : base(connectionString, logger)
        {
            _logger = logger;
            _currentDateTime = currentDateTime;

        }

        public async Task<ApprenticeshipDetails> GetApprenticeship(long apprenticeshipId)
        {
            return await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId);
                parameters.Add("@now", _currentDateTime.UtcNow);

                const string sql = "GetApprenticeshipWithPriceHistory";

                ApprenticeshipDetails result = null;

                await c.QueryAsync<ApprenticeshipDetails, PriceHistoryDetails, ApprenticeshipDetails>
                (sql, (apprenticeship, history) =>
                {
                    if (result == null)
                    {
                        result = apprenticeship;
                    }

                    if (history.ApprenticeshipId != 0)
                    {
                        result.PriceHistory.Add(history);
                    }

                    return apprenticeship;
                }, parameters, commandType: CommandType.StoredProcedure);

                return result;
            });
        }
    }
}
