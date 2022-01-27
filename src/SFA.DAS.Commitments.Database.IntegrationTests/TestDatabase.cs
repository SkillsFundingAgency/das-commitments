using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SFA.DAS.Commitments.Database.IntegrationTests
{
    public static class TestDatabase
    {
        private const string LIVE_DATABASE_NAME = "SFA.DAS.Commitments.Database";
        private const string TEST_DATABASE_NAME = "SFA.DAS.Commitments.Database.Test";

        private static readonly string LiveConnectionString;
        private static readonly string TestConnectionString;

        static TestDatabase()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("connectionStrings.Local.json")
                .Build();
            LiveConnectionString = configuration.GetConnectionString("SqlConnectionString");
            TestConnectionString = configuration.GetConnectionString("SqlConnectionStringTest");
        }

        public static void SetupDatabase()
        {
            DropDatabase();

            using (var connection = new SqlConnection(LiveConnectionString))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var comm = new SqlCommand
                {
                    Connection = connection,
                    CommandText =
                        $@"DBCC CLONEDATABASE ('{LIVE_DATABASE_NAME}', '{TEST_DATABASE_NAME}'); ALTER DATABASE [{TEST_DATABASE_NAME}] SET READ_WRITE;"
                };
                comm.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void DropDatabase()
        {
            using (var connection = new SqlConnection(LiveConnectionString))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var comm = new SqlCommand
                {
                    Connection = connection,
                    CommandText =
                        $@"IF EXISTS( SELECT* FROM sys.databases WHERE name = '{TEST_DATABASE_NAME}') BEGIN ALTER DATABASE [{TEST_DATABASE_NAME}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;  DROP DATABASE [{TEST_DATABASE_NAME}]; END"
                };
                comm.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static int Execute(string sql)
        {
            using (var connection = new SqlConnection(TestConnectionString))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var comm = new SqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };

                var rowsAffected = comm.ExecuteNonQuery();
                connection.Close();
                return rowsAffected;
            }
        }

        public static IEnumerable<T> Query<T>(string sql, DynamicParameters parameters = null, CommandType commandType = CommandType.Text)
        {
            using (var connection = new SqlConnection(TestConnectionString))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var result = connection.Query<T>(sql, parameters, commandType: commandType);
                connection.Close();

                return result;
            }
        }
    }
}
