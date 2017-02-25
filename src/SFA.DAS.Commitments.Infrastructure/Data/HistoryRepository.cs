using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class HistoryRepository : BaseRepository, IHistoryRepository
    {
        private readonly ICommitmentsLogger _logger;

        public HistoryRepository(string connectionString, ICommitmentsLogger logger)
            : base(connectionString)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public async Task CreateCommitmentHistory(CommitmentHistoryItem item)
        {
            if(item.UpdatedByRole == CallerType.Employer )
                _logger.Debug($"Creating history item for commitment: {item.CommitmentId}", commitmentId: item.CommitmentId, accountId: item.UserId);
            else
                _logger.Debug($"Creating history item for commitment: {item.CommitmentId}", commitmentId: item.CommitmentId, providerId: item.UserId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@commitmentId", item.CommitmentId, DbType.Int64);
                parameters.Add("@userId", item.UserId, DbType.Int64);
                parameters.Add("@updatedByRole", item.UpdatedByRole, DbType.Int16);
                parameters.Add("@changeType", item.ChangeType, DbType.Int16);
                parameters.Add("@createdOn", item.CreatedOn, DbType.DateTime);

                using (var trans = connection.BeginTransaction())
                {
                     await connection.QueryAsync<long>(
                        sql:
                        "INSERT INTO [dbo].[CommitmentHistory](CommitmentId, UserId, UpdatedByRole, ChangeType, CreatedOn) " +
                        "VALUES (@commitmentId, @userId, @updatedByRole, @changeType, @createdOn); ",
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: trans);

                    trans.Commit();
                }
                return 1L;
            });
        }

        public async Task CreateApprenticeship(ApprenticeshipHistoryItem item)
        {
            if (item.UpdatedByRole == CallerType.Employer)
                _logger.Debug($"Creating history item for apprenticehsip: {item.ApprenticeshipId}", apprenticeshipId: item.ApprenticeshipId, accountId: item.UserId);
            else
                _logger.Debug($"Creating history item for apprenticehsip: {item.ApprenticeshipId}", apprenticeshipId: item.ApprenticeshipId, providerId: item.UserId);

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@apprenticeshipId", item.ApprenticeshipId, DbType.Int64);
                parameters.Add("@userId", item.UserId, DbType.Int64);
                parameters.Add("@updatedByRole", item.UpdatedByRole, DbType.Int16);
                parameters.Add("@changeType", item.ChangeType, DbType.Int16);
                parameters.Add("@createdOn", item.CreatedOn, DbType.DateTime);

                using (var trans = connection.BeginTransaction())
                {
                    await connection.QueryAsync<long>(
                        sql:
                        "INSERT INTO [dbo].[ApprenticeshipHistory](ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn) " +
                        "VALUES (@apprenticeshipId, @userId, @updatedByRole, @changeType, @createdOn); ",
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: trans);

                    trans.Commit();
                }
                return 1L;
            });
        }
    }
}
