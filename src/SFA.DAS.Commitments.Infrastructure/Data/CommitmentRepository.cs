using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Configuration;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class CommitmentRepository : BaseRepository, ICommitmentRepository
    {
        public CommitmentRepository(CommitmentConfiguration configuration) : base(configuration)
        {
        }

        public async Task Create(Commitment commitment)
        {
            await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@name", commitment.Name, DbType.String);
                parameters.Add("@legalEntityId", commitment.LegalEntityId, DbType.Int64);
                parameters.Add("@accountId", commitment.EmployerAccountId, DbType.Int64);
                parameters.Add("@providerId", commitment.ProviderId, DbType.Int64);

                var trans = c.BeginTransaction();
                var commitmentId = await c.ExecuteAsync(
                    sql: "INSERT INTO [dbo].[Commitment](Name, LegalEntityId, EmployerAccountId, ProviderId) VALUES (@name, @legalEntityId, @accountId, @providerId);",
                    param: parameters,
                    commandType: CommandType.Text, 
                    transaction: trans);

                foreach (var apprenticeship in commitment.Apprenticeships)
                {
                    parameters = new DynamicParameters();
                    parameters.Add("@commitmentId", commitmentId, DbType.Int64);
                    parameters.Add("@cost", apprenticeship.Cost, DbType.Decimal);
                    parameters.Add("@startDate", apprenticeship.StartDate, DbType.DateTime);
                    parameters.Add("@endDate", apprenticeship.EndDate, DbType.DateTime);

                    await c.ExecuteAsync(
                        sql: "INSERT INTO [dbo].[Apprenticeship](CommitmentId, Cost, StartDate, EndDate) VALUES (@commitmentId, @cost, @startDate, @endDate);",
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: trans);
                }

                trans.Commit();
                return commitmentId;
            });
        }
    }
}