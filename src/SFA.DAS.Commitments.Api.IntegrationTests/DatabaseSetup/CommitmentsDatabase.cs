using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastMember;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Infrastructure.Configuration;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class CommitmentsDatabase
    {
        public const int SchemaVersion = 1;

        public const string ApprenticeshipTableName = "[dbo].[Apprenticeship]";
        public const string ApprenticeshipUpdateTableName = "[dbo].[ApprenticeshipUpdate]";
        public const string CommitmentTableName = "[dbo].[Commitment]";
        public const string DataLockStatusTableName = "[dbo].[DataLockStatus]";

        private readonly string _databaseConnectionString;

        public CommitmentsDatabase(string databaseConnectionString)
        {
            _databaseConnectionString = databaseConnectionString;
        }

        public async Task InsertApprenticeships(List<DbSetupApprenticeship> apprenticeships)
        {
            await BulkInsertRows(apprenticeships, ApprenticeshipTableName, new []
            {
                "Id", //todo: incrementing
                "CommitmentId",
                //todo: public string Reference",
                "FirstName",
                "LastName",
                "ULN",
                "TrainingType",
                "TrainingCode",
                "TrainingName",
                "Cost",
                "StartDate",
                "EndDate",
                "AgreementStatus",
                "PaymentStatus",
                "DateOfBirth",
                "NINumber",
                "EmployerRef",
                "ProviderRef",
                "CreatedOn",
                "AgreedOn",
                "PaymentOrder"

                //"PauseDate",
                //"StopDate",
                //"HasHadDataLockSuccess"
            });
        }
        
        public async Task InsertApprenticeshipUpdates(List<DbSetupApprenticeshipUpdate> apprenticeshipUpdates)
        {
            await BulkInsertRows(apprenticeshipUpdates, ApprenticeshipUpdateTableName, new[]
            {
                "Id", "ApprenticeshipId", "Originator", "Status", "FirstName", "LastName",
                "TrainingType", "TrainingCode", "TrainingName", "Cost", "StartDate",
                "EndDate", "DateOfBirth", "CreatedOn", "UpdateOrigin", "EffectiveFromDate",
                "EffectiveToDate"
            });
        }

        public async Task InsertCommitments(List<DbSetupCommitment> commitments)
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

        public async Task InsertDataLockStatuses(List<DbSetupDataLockStatus> dataLockStatuses)
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

        public async Task BulkInsertRows<T>(List<T> rowData, string tableName, string[] columnNamesInTableOrder)
        {
            using (var connection = new SqlConnection(_databaseConnectionString))
            {
                await connection.OpenAsync();
                using (var bcp = new SqlBulkCopy(connection))
                using (var reader = ObjectReader.Create(rowData, columnNamesInTableOrder))
                {
                    bcp.DestinationTableName = tableName;
                    bcp.EnableStreaming = true;
                    bcp.BatchSize = 10000;
                    bcp.NotifyAfter = 1000;
                    bcp.SqlRowsCopied += async (sender, e) => await TestContext.Progress.WriteLineAsync($"Copied {e.RowsCopied} rows into {tableName}.");

                    await bcp.WriteToServerAsync(reader);
                }
            }
        }

        public async Task<long?> LastId(string tableName)
        {
            using (var connection = new SqlConnection(_databaseConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand($"SELECT MAX(Id) FROM {tableName}", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return result == DBNull.Value ? null : (long?) result;
                }
            }
        }

        //store in JobProgress or a seperate table, either in db project or seperate??
        // test apprenticeship ids are not job progresses. change tablename to something else?
        public async Task<T> GetJobProgress<T>(string columnName)
        {
            using (var connection = new SqlConnection(_databaseConnectionString))
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
            using (var connection = new SqlConnection(_databaseConnectionString))
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

        public async Task<long> FirstNewId(string tableName)
        {
            var latestIdInDatabase = await LastId(tableName);
            return (latestIdInDatabase ?? 0) + 1;
        }

        public async Task<int> CountOfRows(string tableName)
        {
            using (var connection = new SqlConnection(_databaseConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand($"SELECT Count(1) FROM {tableName}", connection))
                {
                    return (int)await command.ExecuteScalarAsync();
                }
            }
        }
    }
}