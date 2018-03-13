using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastMember;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;
using SFA.DAS.Commitments.Infrastructure.Configuration;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class CommitmentsDatabase
    {
        // intial version will be null
        public static readonly int? SchemaVersion = null;

        public const string DatabaseName = "SFA.DAS.Commitments.IntegrationTest";

        public const string ApprenticeshipTableName = "[dbo].[Apprenticeship]";
        public const string ApprenticeshipUpdateTableName = "[dbo].[ApprenticeshipUpdate]";
        public const string CommitmentTableName = "[dbo].[Commitment]";
        public const string DataLockStatusTableName = "[dbo].[DataLockStatus]";

        private readonly string _databaseConnectionString;

        public CommitmentsDatabase(string databaseConnectionString)
        {
            _databaseConnectionString = databaseConnectionString;
        }

        //        public async Task ClearData()
        //        {
        //            using (var connection = new SqlConnection(_databaseConnectionString))
        //            {
        //                await connection.OpenAsync();
        //                using (var command = new SqlCommand(
        //$@"truncate table {ApprenticeshipTableName} go
        //truncate table {ApprenticeshipUpdateTableName} go
        //truncate table {CommitmentTableName} go
        //truncate table {DataLockStatusTableName} go", connection))
        //                {
        //                    await command.ExecuteNonQueryAsync();
        //                }
        //            }
        //        }


        //public void Drop()
        //{
        //    using (var sqlConnection = new SqlConnection(_databaseConnectionString))
        //    {
        //        var serverConnection = new ServerConnection(sqlConnection);
        //        var server = new Server(serverConnection);
        //        server.KillDatabase(sqlConnection.Database);
        //    }
        //}

        public async Task InsertApprenticeships(IEnumerable<DbSetupApprenticeship> apprenticeships)
        {
            await BulkInsertRows(apprenticeships, ApprenticeshipTableName, new []
            {
                // "Id" needs to be in this array, but the values supplied in the passed-in collection are ignored
                "Id",
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

                //todo: these are in the table so we need to write them
                //"StopDate",
                //"PauseDate",
                //"HasHadDataLockSuccess"
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

        public async Task BulkInsertRows<T>(IEnumerable<T> rowData, string tableName, string[] columnNamesInTableOrder)
        {
            using (var connection = new SqlConnection(_databaseConnectionString))
            {
                await connection.OpenAsync();
                using (var bcp = new SqlBulkCopy(connection))
                using (var reader = ObjectReader.Create(rowData, columnNamesInTableOrder))
                {
                    bcp.DestinationTableName = tableName;
                    bcp.EnableStreaming = true;
                    bcp.BatchSize = 5000;
                    bcp.NotifyAfter = 1000;
                    bcp.SqlRowsCopied += async (sender, e) => await SetUpFixture.LogProgress($"Copied {e.RowsCopied} rows into {tableName}.");

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

        //todo: reuse existing repository? perhaps better not to use existing code, as then test code could be wrong because of bug in sut code

        public async Task<long> GetEmployerId(long apprenticeshipId)
        {
            using (var connection = new SqlConnection(_databaseConnectionString))
            {
                await connection.OpenAsync();
                // no need for param as apprenticeshipId comes from test code not user
                //todo: find eisting functionality to do this?
                using (var command = new SqlCommand(
                    $@"select EmployerAccountId from dbo.Commitment c
                    join dbo.Apprenticeship a on c.Id = a.CommitmentId
                    where a.Id = {apprenticeshipId}", connection))
                {
                    return (long)await command.ExecuteScalarAsync();
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