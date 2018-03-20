using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using FastMember;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;

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
                    bcp.SqlRowsCopied += async (sender, e) => await TestSetup.LogProgress($"Copied {e.RowsCopied} rows into {tableName}.");

                    await bcp.WriteToServerAsync(reader);
                }
            }
        }

        public async Task<long?> LastId(string tableName, string columnName = "Id")
        {
            using (var connection = new SqlConnection(DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand($"SELECT MAX({columnName}) FROM {tableName}", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return result == DBNull.Value ? null : (long?)result;
                }
            }
        }

        public async Task<long> FirstNewId(string tableName, string columnName = "Id")
        {
            var latestIdInDatabase = await LastId(tableName, columnName);
            return (latestIdInDatabase ?? 0) + 1;
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
