﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NLog;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class CommitmentRepository : BaseRepository, ICommitmentRepository
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public CommitmentRepository(string databaseConnectionString) : base(databaseConnectionString) {}

        public async Task<long> Create(Commitment commitment)
        {
            return await WithConnection(async connection =>
            {
                long commitmentId;

                var parameters = new DynamicParameters();
                parameters.Add("@reference", commitment.Reference, DbType.String);
                parameters.Add("@legalEntityId", commitment.LegalEntityId, DbType.String);
                parameters.Add("@legalEntityName", commitment.LegalEntityName, DbType.String);
                parameters.Add("@accountId", commitment.EmployerAccountId, DbType.Int64);
                parameters.Add("@providerId", commitment.ProviderId, DbType.Int64);
                parameters.Add("@providerName", commitment.ProviderName, DbType.String);
                parameters.Add("@commitmentStatus", commitment.CommitmentStatus, DbType.Int16);
                parameters.Add("@editStatus", commitment.EditStatus, DbType.Int16);
                parameters.Add("@id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);

                using (var trans = connection.BeginTransaction())
                {
                    commitmentId = (await connection.QueryAsync<long>(
                        sql:
                            "INSERT INTO [dbo].[Commitment](Reference, LegalEntityId, LegalEntityName, EmployerAccountId, ProviderId, ProviderName, CommitmentStatus, EditStatus, CreatedOn) " +
                            "VALUES (@reference, @legalEntityId, @legalEntityName, @accountId, @providerId, @providerName, @commitmentStatus, @editStatus, @createdOn); " +
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
            return await WithConnection(async connection => { return await CreateApprenticeship(connection, null, apprenticeship); });
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
                    sql: $"SELECT c.*, a.* FROM [dbo].[Commitment] c LEFT JOIN [dbo].Apprenticeship a ON a.CommitmentId = c.Id WHERE c.Id = @id AND c.CommitmentStatus <> {(int)CommitmentStatus.Deleted};",
                    param: parameters,
                    map: mapper.Map(lookup, x => x.Id, x => x.Apprenticeships));

                return lookup.Values.SingleOrDefault();
            });
        }

        public async Task<IList<CommitmentSummary>> GetByEmployer(long accountId)
        {
            return await GetByIdentifier("EmployerAccountId", accountId);
        }

        public async Task<IList<CommitmentSummary>> GetByProvider(long providerId)
        {
            return await GetByIdentifier("ProviderId", providerId);
        }

        public async Task UpdateCommitmentStatus(long commitmentId, CommitmentStatus commitmentStatus)
        {
            Logger.Debug($"Updating commitment {commitmentId} commitment status to {commitmentStatus}");

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitmentId, DbType.Int64);
                parameters.Add("@commitmentStatus", commitmentStatus, DbType.Int16);

                var returnCode = await connection.ExecuteAsync(
                    sql: "UPDATE [dbo].[Commitment] SET CommitmentStatus = @commitmentStatus WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateCommitmentStatus(long commitmentId, EditStatus editStatus)
        {
            Logger.Debug($"Updating commitment {commitmentId} edit status to {editStatus}");

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitmentId, DbType.Int64);
                parameters.Add("@editStatus", editStatus, DbType.Int16);

                var returnCode = await connection.ExecuteAsync(
                    sql: "UPDATE [dbo].[Commitment] SET EditStatus = @editStatus WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller)
        {
            await WithConnection(async connection =>
            {
                var parameters = GetApprenticeshipUpdateCreateParameters(apprenticeship);
                parameters.Add("@id", apprenticeship.Id, DbType.Int64);

                var sql = GetUpdateApprenticeshipSql(caller.CallerType);

                var returnCode = await connection.ExecuteAsync(
                    sql: sql,
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus)
        {
            Logger.Debug($"Updating apprenticeship {apprenticeshipId} for commitment {commitmentId} payment status to {paymentStatus}");

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@paymentStatus", paymentStatus, DbType.Int16);

                var returnCode = await connection.ExecuteAsync(
                    sql:
                        "UPDATE [dbo].[Apprenticeship] SET PaymentStatus = @paymentStatus " +
                        "WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, AgreementStatus agreementStatus)
        {
            Logger.Debug($"Updating apprenticeship {apprenticeshipId} for commitment {commitmentId} agreement status to {agreementStatus}");

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", apprenticeshipId, DbType.Int64);
                parameters.Add("@agreementStatus", agreementStatus, DbType.Int16);

                var returnCode = await connection.ExecuteAsync(
                    sql:
                        "UPDATE [dbo].[Apprenticeship] SET AgreementStatus = @agreementStatus " +
                        "WHERE Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);

                return returnCode;
            });
        }

        public async Task UpdateReference(long commitmentId, string hashValue)
        {
            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", commitmentId, DbType.Int64);
                parameters.Add("@reference", hashValue, DbType.String);

                var returnCode = await connection.ExecuteAsync(
                    sql: "UPDATE [dbo].[Commitment] SET Reference = @reference WHERE Id = @id;",
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
            var parameters = GetApprenticeshipUpdateCreateParameters(apprenticeship);

            var apprenticeshipId = (await connection.QueryAsync<long>(
                sql:
                    "INSERT INTO [dbo].[Apprenticeship](CommitmentId, FirstName, LastName, DateOfBirth, NINumber, ULN, TrainingType, TrainingCode, TrainingName, Cost, StartDate, EndDate, PaymentStatus, AgreementStatus, EmployerRef, ProviderRef, CreatedOn) " +
                    "VALUES (@commitmentId, @firstName, @lastName, @dateOfBirth, @niNumber, @uln, @trainingType, @trainingCode, @trainingName, @cost, @startDate, @endDate, @paymentStatus, @agreementStatus, @employerRef, @providerRef, @createdOn); " +
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
            parameters.Add("@dateOfBirth", apprenticeship.DateOfBirth, DbType.DateTime);
            parameters.Add("@niNumber", apprenticeship.NINumber, DbType.String);
            parameters.Add("@trainingType", apprenticeship.TrainingType, DbType.Int32);
            parameters.Add("@trainingCode", apprenticeship.TrainingCode, DbType.String);
            parameters.Add("@trainingName", apprenticeship.TrainingName, DbType.String);
            parameters.Add("@uln", apprenticeship.ULN, DbType.String);
            parameters.Add("@cost", apprenticeship.Cost, DbType.Decimal);
            parameters.Add("@startDate", apprenticeship.StartDate, DbType.DateTime);
            parameters.Add("@endDate", apprenticeship.EndDate, DbType.DateTime);
            parameters.Add("@paymentStatus", apprenticeship.PaymentStatus, DbType.Int16);
            parameters.Add("@agreementStatus", apprenticeship.AgreementStatus, DbType.Int16);
            parameters.Add("@employerRef", apprenticeship.EmployerRef, DbType.String);
            parameters.Add("@providerRef", apprenticeship.ProviderRef, DbType.String);
            parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);
            return parameters;
        }

        private Task<IList<CommitmentSummary>> GetByIdentifier(string identifierName, long identifierValue)
        {
            return WithConnection<IList<CommitmentSummary>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add($"@id", identifierValue);

                var results = await c.QueryAsync<CommitmentSummary>(
                    sql: $"SELECT * FROM [dbo].[CommitmentSummary] WHERE {identifierName} = @id AND CommitmentStatus <> {(int)CommitmentStatus.Deleted};",
                    param: parameters);

                return results.ToList();
            });
        }

        private static string GetUpdateApprenticeshipSql(CallerType callerType)
        {
            var refItem = callerType == CallerType.Employer ? "EmployerRef = @employerRef" : "ProviderRef = @providerRef";

            return "UPDATE [dbo].[Apprenticeship] " +
                   "SET CommitmentId = @commitmentId, FirstName = @firstName, LastName = @lastName, DateOfBirth = @dateOfBirth, NINUmber = @niNumber, " +
                   "ULN = @uln, TrainingType = @trainingType, TrainingCode = @trainingCode, TrainingName = @trainingName, Cost = @cost, " +
                   "StartDate = @startDate, EndDate = @endDate, PaymentStatus = @paymentStatus, AgreementStatus = @agreementStatus, " +
                   $"{refItem} WHERE Id = @id;";
        }
    }
}
