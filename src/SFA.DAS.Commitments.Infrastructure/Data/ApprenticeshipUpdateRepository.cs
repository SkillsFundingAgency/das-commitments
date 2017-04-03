using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
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

        public ApprenticeshipUpdateRepository(string connectionString, ICommitmentsLogger logger, IApprenticeshipUpdateTransactions apprenticeshipUpdateTransactions) : base(connectionString)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if(apprenticeshipUpdateTransactions==null)
                throw new ArgumentNullException(nameof(apprenticeshipUpdateTransactions));

            _logger = logger;
            _apprenticeshipUpdateTransactions = apprenticeshipUpdateTransactions;
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
    }
}
