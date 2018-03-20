using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Dac;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class DatabaseManagement
    {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public DatabaseManagement(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            _databaseName = builder.InitialCatalog;
            builder.Remove("Initial Catalog");
            _connectionString = builder.ConnectionString;
        }

        public async Task<bool> Exists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand($"select name from sys.databases where name = '{_databaseName}'", connection))
                {
                    return (await command.ExecuteScalarAsync()) != null;
                }
            }
        }

        //public void Create(string databaseName)
        //{
        //    var server = new Server();
        //    var db = new Database(server, databaseName); //todo: setup params, esp. for azure sql
        //    db.Create();
        //}

        public void Publish()
        {
            var dacServices = new DacServices(_connectionString);

            dacServices.Message += async (sender, e) => await TestLog.Progress($"Deploy database: {e.Message}");
            dacServices.ProgressChanged += async (sender, e) => await TestLog.Progress($"Deploy database: {e.Message}");

            //todo: get current config DEBUG?
            var dacpacPath = $@"{TestContext.CurrentContext.TestDirectory}\..\..\..\SFA.DAS.Commitments.Database\bin\Debug\SFA.DAS.Commitments.Database.dacpac";
            var dacPackage = DacPackage.Load(dacpacPath);

            // these are ignored! it warns debug variable is missing, and it doesn't create a new database! others are reporting similar on t'internet
            // if we can't get the options to work, we can do some of the stuff in other ways (e.g. drop/creeate using SMO)
            // people also report that the options aren't ignored if you generate a script (and execute that)
            var dacDeployOptions = new DacDeployOptions
            {
                CreateNewDatabase = true
                //DeployDatabaseInSingleUserMode = true
            };
            dacDeployOptions.SqlCommandVariableValues.Add("debug", "false");

            // https://stackoverflow.com/questions/31041788/publish-dacpac-in-single-user-mode-using-microsoft-sqlserver-dac-dacservices

            //todo: set upgradeExisting to false, as we won't do upgrades only recreations
            dacServices.Deploy(dacPackage, _databaseName, true, dacDeployOptions);
        }

        public void Kill()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var serverConnection = new ServerConnection(sqlConnection);
                var server = new Server(serverConnection);

                // against SQL Azure database, throws Microsoft.SqlServer.Management.Smo.UnsupportedFeatureException
                // { "This object is not supported on Microsoft Azure SQL Database."}
                server.KillDatabase(_databaseName);
            }
        }

        public void KillAzure()
        {
            // call this instead?
            // https://docs.microsoft.com/en-us/powershell/module/azure/remove-azuresqldatabase?view=azuresmps-4.0.0
            // https://blogs.msdn.microsoft.com/kebab/2014/04/28/executing-powershell-scripts-from-c/
            // i have helper code to call powershell cmdlets from c sharp (in NCS code). could reuse that
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var serverConnection = new ServerConnection(sqlConnection);
                var server = new Server(serverConnection);
                if (!server.Databases.Contains(_databaseName))
                    return;

                // requires Microsoft.SqlServer.BatchParser
                // https://social.msdn.microsoft.com/Forums/sqlserver/en-US/7a71121c-83b1-49b4-ad30-3a5f20e7afbf/smo-2017-microsoftsqlserverbatchparserdll-load-error?forum=sqlsmoanddmo
                // not doing this presumably means this'll fail if anything is already connected?

                //serverConnection.ExecuteNonQuery($"ALTER DATABASE {_databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
                var database = server.Databases[_databaseName];
                //database.Alter();
                database.Drop();
            }
        }
    }
}
