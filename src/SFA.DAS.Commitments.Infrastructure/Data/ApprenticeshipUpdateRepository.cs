using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class ApprenticeshipUpdateRepository : BaseRepository, IApprenticeshipUpdateRepository
    {
        private readonly ICommitmentsLogger _logger;

        public ApprenticeshipUpdateRepository(string connectionString, ICommitmentsLogger logger) : base(connectionString)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public async Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long apprenticeshipId)
        {
            return await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@apprenticeshipId", apprenticeshipId);

                var results = await connection.QueryAsync<ApprenticeshipUpdate>(
                    sql: $"[dbo].[GetApprenticeshipUpdates]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return results.SingleOrDefault();
            });
        }

        public async Task CreateApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate)
        {
            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@apprenticeshipId", apprenticeshipUpdate.ApprenticeshipId);

                parameters.Add("@Originator", apprenticeshipUpdate.ApprenticeshipId);
                parameters.Add("@FirstName", apprenticeshipUpdate.FirstName);
                parameters.Add("@LastName", apprenticeshipUpdate.LastName);
                parameters.Add("@ULN", apprenticeshipUpdate.ULN);
                parameters.Add("@TrainingType", apprenticeshipUpdate.TrainingType);
                parameters.Add("@TrainingCode", apprenticeshipUpdate.TrainingCode);
                parameters.Add("@TrainingName", apprenticeshipUpdate.TrainingName);
                parameters.Add("@Cost", apprenticeshipUpdate.Cost);
                parameters.Add("@StartDate", apprenticeshipUpdate.StartDate);
                parameters.Add("@EndDate", apprenticeshipUpdate.EndDate);
                parameters.Add("@DateOfBirth", apprenticeshipUpdate.DateOfBirth);

                var returnCode = await connection.ExecuteAsync(
                    sql: $"[dbo].[CreateApprenticeshipUpdate]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return returnCode;
            });
        }
    }
}
