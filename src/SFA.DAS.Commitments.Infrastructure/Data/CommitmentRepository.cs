using System;
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

        public async Task Create(Commitment commitment)
        {
            await WithConnection(async c =>
            {
                long commitmentId;

                var parameters = new DynamicParameters();
                parameters.Add("@name", commitment.Name, DbType.String);
                parameters.Add("@legalEntityId", commitment.LegalEntityId, DbType.Int64);
                parameters.Add("@accountId", commitment.EmployerAccountId, DbType.Int64);
                parameters.Add("@providerId", commitment.ProviderId, DbType.Int64);
                parameters.Add("@id", dbType: DbType.Int64, direction: ParameterDirection.Output);

                using (var trans = c.BeginTransaction())
                {
                    commitmentId = await c.ExecuteAsync(
                        sql:
                            "INSERT INTO [dbo].[Commitment](Name, LegalEntityId, EmployerAccountId, ProviderId) VALUES (@name, @legalEntityId, @accountId, @providerId);",
                        param: parameters,
                        commandType: CommandType.Text,
                        transaction: trans);

                    foreach (var apprenticeship in commitment.Apprenticeships)
                    {
                        parameters = new DynamicParameters();
                        parameters.Add("@commitmentId", commitmentId, DbType.Int64);
                        parameters.Add("@apprenticeName", apprenticeship.ApprenticeName, DbType.String);
                        //parameters.Add("@traingingId", apprenticeship.TrainingId, DbType.String); TODO: LWA - Need to decide on datatype
                        parameters.Add("@uln", apprenticeship.ULN, DbType.String);
                        parameters.Add("@cost", apprenticeship.Cost, DbType.Decimal);
                        parameters.Add("@startDate", apprenticeship.StartDate, DbType.DateTime);
                        parameters.Add("@endDate", apprenticeship.EndDate, DbType.DateTime);
                        parameters.Add("@status", apprenticeship.Status, DbType.Int16);
                        parameters.Add("@agreementStatus", apprenticeship.AgreementStatus, DbType.Int16);


                        await c.ExecuteAsync(
                            sql:
                                "INSERT INTO [dbo].[Apprenticeship](CommitmentId, ApprenticeName, ULN, Cost, StartDate, EndDate, Status, AgreementStatus) VALUES (@commitmentId, @apprenticeName, @uln, @cost, @startDate, @endDate, @status, @agreementStatus);",
                            param: parameters,
                            commandType: CommandType.Text,
                            transaction: trans);
                    }

                    trans.Commit();
                }
                return commitmentId;
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

        private Task<IList<Commitment>> GetByIdentifier(string identifierName, long identifierValue)
        {
            var mapper = new ParentChildrenMapper<Commitment, Apprenticeship>();

            return WithConnection<IList<Commitment>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@id", identifierValue);

                var results = await c.QueryAsync<Commitment> (
                    sql: $"SELECT * FROM [dbo].[Commitment] WHERE {identifierName} = @id;",
                    param: parameters);

                return results.ToList();
            });
        }
    }
}