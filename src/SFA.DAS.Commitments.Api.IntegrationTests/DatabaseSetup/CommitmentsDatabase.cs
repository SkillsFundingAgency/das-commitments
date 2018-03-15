using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class CommitmentsDatabase : Database
    {
        // intial version will be null
        public static readonly int? SchemaVersion = null;

        public const string ApprenticeshipTableName = "[dbo].[Apprenticeship]";
        public const string ApprenticeshipUpdateTableName = "[dbo].[ApprenticeshipUpdate]";
        public const string CommitmentTableName = "[dbo].[Commitment]";
        public const string DataLockStatusTableName = "[dbo].[DataLockStatus]";

        public CommitmentsDatabase(string databaseConnectionString)
            : base(databaseConnectionString)
        {
        }

        public async Task InsertApprenticeships(IEnumerable<DbSetupApprenticeship> apprenticeships)
        {
            await BulkInsertRows(apprenticeships, ApprenticeshipTableName, new []
            {
                // "Id" needs to be in this array, but the values supplied in the passed-in collection are ignored
                "Id", "CommitmentId", "FirstName", "LastName", "ULN", "TrainingType", "TrainingCode",
                "TrainingName", "Cost", "StartDate", "EndDate", "AgreementStatus", "PaymentStatus",
                "DateOfBirth", "NINumber", "EmployerRef", "ProviderRef", "CreatedOn", "AgreedOn",
                "PaymentOrder", "StopDate", "PauseDate", "HasHadDataLockSuccess"
            });
        }
        
        public async Task InsertApprenticeshipUpdates(IEnumerable<DbSetupApprenticeshipUpdate> apprenticeshipUpdates)
        {
            await BulkInsertRows(apprenticeshipUpdates, ApprenticeshipUpdateTableName, new[]
            {
                "Id", "ApprenticeshipId", "Originator", "Status", "FirstName", "LastName",
                "TrainingType", "TrainingCode", "TrainingName", "Cost", "StartDate",
                "EndDate", "DateOfBirth", "CreatedOn", "UpdateOrigin", "EffectiveFromDate",
                "EffectiveToDate"
            });
        }

        public async Task InsertCommitments(IEnumerable<DbSetupCommitment> commitments)
        {
            await BulkInsertRows(commitments, CommitmentTableName, new[]
            {
                "Id", "Reference", "EmployerAccountId", "LegalEntityId", "LegalEntityName",
                "LegalEntityAddress", "LegalEntityOrganisationType", "ProviderId",
                "ProviderName", "CommitmentStatus", "EditStatus", "CreatedOn",
                "LastAction", "LastUpdatedByEmployerName", "LastUpdatedByEmployerEmail",
                "LastUpdatedByProviderName","LastUpdatedByProviderEmail"
            });
        }

        public async Task InsertDataLockStatuses(IEnumerable<DbSetupDataLockStatus> dataLockStatuses)
        {
            await BulkInsertRows(dataLockStatuses, DataLockStatusTableName, new[]
            {
                "Id", "DataLockEventId", "DataLockEventDatetime", "PriceEpisodeIdentifier",
                "ApprenticeshipId", "IlrTrainingCourseCode", "IlrTrainingType",
                "IlrActualStartDate", "IlrEffectiveFromDate", "IlrPriceEffectiveToDate",
                "IlrTotalCost", "ErrorCode", "Status", "TriageStatus",
                "ApprenticeshipUpdateId", "IsResolved", "EventStatus", "IsExpired",
                "Expired"
            });
        }

        public async Task<long> GetEmployerId(long apprenticeshipId)
        {
            using (var connection = new SqlConnection(DatabaseConnectionString))
            {
                await connection.OpenAsync();
                // no need for param, as apprenticeshipId comes from test code not user
                using (var command = new SqlCommand(
                    $@"select EmployerAccountId from dbo.Commitment c
                    join dbo.Apprenticeship a on c.Id = a.CommitmentId
                    where a.Id = {apprenticeshipId}", connection))
                {
                    return (long)await command.ExecuteScalarAsync();
                }
            }
        }

        public async Task<T> GetJobProgress<T>(string columnName)
        {
            using (var connection = new SqlConnection(DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand($"SELECT {columnName} FROM [dbo].[JobProgress]", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    //todo: LastId returns DbNull, this just returns null
                    //https://stackoverflow.com/questions/7927211/executescalar-returns-null-or-dbnull-development-or-production-server
                    //ExecuteScalar returns DBNull for null value from query and null for no result
                    return result == DBNull.Value ? default(T) : (T)result;
                }
            }
        }

        public async Task SetJobProgress<T>(string columnName, T columnValue)
        {
            using (var connection = new SqlConnection(DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
$@"MERGE [dbo].[JobProgress] WITH(HOLDLOCK) as target
using (values(@parameter)) as source (sourceColumn)
on target.Lock = 'X'
when matched then
    update set {columnName} = source.sourceColumn
when not matched then
insert({columnName}) values(source.sourceColumn); ", connection))
                {
                    command.Parameters.AddWithValue("@parameter", columnValue);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<long> GetRandomApprenticeshipId(HashSet<long> exclude = null)
        {
            using (var connection = new SqlConnection(DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("select top 1 Id FROM Apprenticeship order by NEWID()", connection))
                {
                    return (long)await command.ExecuteScalarAsync();
                }
            }
        }
    }
}