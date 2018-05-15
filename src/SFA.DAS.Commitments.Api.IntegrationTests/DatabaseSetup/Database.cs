using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using FastMember;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class Database
    {
        protected readonly string DatabaseConnectionString;

        public Database(string databaseConnectionString)
        {
            DatabaseConnectionString = databaseConnectionString;
        }

        public async Task BulkInsertRows<T>(IEnumerable<T> rowData, string tableName, string[] columnNamesInTableOrder)
        {
            using (var connection = new SqlConnection(DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var bcp = new SqlBulkCopy(connection))
                using (var reader = ObjectReader.Create(rowData, columnNamesInTableOrder))
                {
                    bcp.DestinationTableName = tableName;
                    bcp.EnableStreaming = true;
                    bcp.BatchSize = 5000;
                    bcp.NotifyAfter = 1000;
                    bcp.BulkCopyTimeout = 60;
                    bcp.SqlRowsCopied += async (sender, e) => await TestLog.Progress($"Copied {e.RowsCopied} rows into {tableName}.");

                    await bcp.WriteToServerAsync(reader);
                }
            }
        }

        public async Task<long> NextId(string tableName)
        {
            using (var connection = new SqlConnection(DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
$@"declare @i bigint
set @i=(select IDENT_CURRENT('{tableName}'))
if (@i = 1 AND not exists (select 1 from {tableName}))
  set @i=0
select @i", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return (long)result + 1L;
                }
            }
        }

        public async Task<int> CountOfRows(string tableName)
        {
            using (var connection = new SqlConnection(DatabaseConnectionString))
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
