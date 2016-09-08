using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Infrastructure.Configuration;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class CommitmentRepository : BaseRepository, ICommitmentRepository
    {
        public CommitmentRepository(CommitmentConfiguration configuration)
            : base(configuration)
        {
        }

        public async Task<long> Create(Commitment commitment)
        {
            return await WithConnection(async connection =>
            {
                long commitmentId;

                var parameters = new DynamicParameters();
                parameters.Add("@name", commitment.Name, DbType.String);
                parameters.Add("@legalEntityId", commitment.LegalEntityId, DbType.Int64);
                parameters.Add("@accountId", commitment.EmployerAccountId, DbType.Int64);
                parameters.Add("@providerId", commitment.ProviderId, DbType.Int64);
                parameters.Add("@id", dbType: DbType.Int64, direction: ParameterDirection.Output);

                using (var trans = connection.BeginTransaction())
                {
                    commitmentId = (await connection.QueryAsync<long>(
                        sql:
                            "INSERT INTO [dbo].[Commitment](Name, LegalEntityId, EmployerAccountId, ProviderId) " +
                            "VALUES (@name, @legalEntityId, @accountId, @providerId); " +
                            "SELECT CAST(SCOPE_IDENTITY() as int);",
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: trans)).Single();

                    foreach (var apprenticeship in commitment.Apprenticeships)
                    {
                        apprenticeship.CommitmentId = commitmentId;
                        var appenticeshipId = await CreateApprenticeship(connection, trans, apprenticeship);
                    }

                    trans.Commit();
                }
                return commitmentId;
            });
        }

        public async Task<long> CreateApprenticeship(Apprenticeship apprenticeship)
        {
            return await WithConnection(async connection =>
            {
                return await CreateApprenticeship(connection, null, apprenticeship);
            });
        }

        public async Task<Commitment> GetById(long id)
        {
            var mapper = new ParentChildrenMapper<Commitment, Apprenticeship>();

            return await WithConnection<Commitment>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@id", id);

                var lookup = new Dictionary<object, Commitment>();
                var results = await c.QueryAsync(
                    sql: $"SELECT c.*, a.* FROM [dbo].[Commitment] c LEFT JOIN [dbo].Apprenticeship a ON a.CommitmentId = c.Id WHERE c.Id = @id;",
                    param: parameters,
                    map: mapper.Map(lookup, x => x.Id, x => x.Apprenticeships));

                return lookup.Values.SingleOrDefault();
            });
        }

        public async Task<IList<Commitment>> GetByEmployer(long accountId)
        {
            return await GetByIdentifier("EmployerAccountId", accountId);
        }

        public async Task<IList<Commitment>> GetByProvider(long providerId)
        {
            return await GetByIdentifier("ProviderId", providerId);
        }

        public async Task UpdateApprenticeship(Apprenticeship apprenticeship)
        {
            await WithConnection(async connection =>
            {
                DynamicParameters parameters = GetApprenticeshipUpdateCreateParameters(apprenticeship);
                parameters.Add("@id", apprenticeship.Id, DbType.Int64);

                // TODO: LWA - Do we need to check the return code?
                var returnCode = await connection.ExecuteAsync(
                    sql:
                        "UPDATE [dbo].[Apprenticeship] SET CommitmentId = @commitmentId, ApprenticeName = @apprenticeName, ULN = @uln, TrainingId = @trainingId, Cost = @cost, StartDate = @startDate, EndDate = @endDate, Status = @status, AgreementStatus = @agreementStatus " +
                        "WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task<Apprenticeship> GetApprenticeship(long id)
        {
            var results = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", id);

                return await c.QueryAsync<Apprenticeship>(
                    sql: $"SELECT * FROM [dbo].[Apprenticeship] WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);
            });

            return results.SingleOrDefault();
        }

        private static async Task<long> CreateApprenticeship(IDbConnection connection, IDbTransaction trans, Apprenticeship apprenticeship)
        {
            DynamicParameters parameters = GetApprenticeshipUpdateCreateParameters(apprenticeship);

            var apprenticeshipId = (await connection.QueryAsync<long>(
                sql:
                    "INSERT INTO [dbo].[Apprenticeship](CommitmentId, FirstName, LastName, ULN, TrainingId, Cost, StartDate, EndDate, Status, AgreementStatus) " +
                    "VALUES (@commitmentId, @firstName, @lastName, @uln, @trainingId, @cost, @startDate, @endDate, @status, @agreementStatus); " +
                    "SELECT CAST(SCOPE_IDENTITY() as int);",
                param: parameters,
                commandType: CommandType.Text,
                transaction: trans)).Single();

            return apprenticeshipId;
        }

        private static DynamicParameters GetApprenticeshipUpdateCreateParameters(Apprenticeship apprenticeship)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", apprenticeship.CommitmentId, DbType.Int64);
            parameters.Add("@firstName", apprenticeship.FirstName, DbType.String);
            parameters.Add("@lastName", apprenticeship.LastName, DbType.String);
            //TODO: LWA - Need to decide on datatype
            parameters.Add("@trainingId", apprenticeship.TrainingId, DbType.String); 
            parameters.Add("@uln", apprenticeship.ULN, DbType.String);
            parameters.Add("@cost", apprenticeship.Cost, DbType.Decimal);
            parameters.Add("@startDate", apprenticeship.StartDate, DbType.DateTime);
            parameters.Add("@endDate", apprenticeship.EndDate, DbType.DateTime);
            parameters.Add("@status", apprenticeship.Status, DbType.Int16);
            parameters.Add("@agreementStatus", apprenticeship.AgreementStatus, DbType.Int16);

            return parameters;
        }

        private Task<IList<Commitment>> GetByIdentifier(string identifierName, long identifierValue)
        {
            var mapper = new ParentChildrenMapper<Commitment, Apprenticeship>();

            return WithConnection<IList<Commitment>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@id", identifierValue);

                var results = await c.QueryAsync<Commitment>(
                    sql: $"SELECT * FROM [dbo].[Commitment] WHERE {identifierName} = @id;",
                    param: parameters);

                return results.ToList();
            });
        }
    }
}