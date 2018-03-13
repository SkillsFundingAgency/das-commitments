using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Dac;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;

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

        public void Create(string databaseName)
        {
            var server = new Server();
            var db = new Database(server, databaseName); //todo: setup params, esp. for azure sql
            db.Create();
        }

        public void Publish()
        {
            var dacServices = new DacServices(_connectionString);

            //todo: check async version
            dacServices.Message += async (sender, e) => await SetUpFixture.LogProgress($"Deploy database: {e.Message}");
            dacServices.ProgressChanged += async (sender, e) => await SetUpFixture.LogProgress($"Deploy database: {e.Message}");

            //todo: get current config DEBUG?
            var dacpacPath = $@"{TestContext.CurrentContext.TestDirectory}\..\..\..\SFA.DAS.Commitments.Database\bin\Debug\SFA.DAS.Commitments.Database.dacpac";
            var dacPackage = DacPackage.Load(dacpacPath);

            // these are ignored! it warns debug variable is missing, and it doesn't create a new database! others are reporting similar on t'internet
            // if we can't get the options to work, we can do some of the stuff in other ways (e.g. drop/creeate using SMO)
            // people also report that the options aren't ignored if you generate a script (and execute that)
            var dacDeployOptions = new DacDeployOptions
            {
                CreateNewDatabase = true
            };
            dacDeployOptions.SqlCommandVariableValues.Add("debug", "false");
            //dbDeployOptions.DeployDatabaseInSingleUserMode = true;
            //dbDeployOptions.CreateNewDatabase = true;

            // https://stackoverflow.com/questions/31041788/publish-dacpac-in-single-user-mode-using-microsoft-sqlserver-dac-dacservices

            //todo: set upgradeExisting to false, as we won't do upgrades only recreations
            dacServices.Deploy(dacPackage, _databaseName, true, dacDeployOptions);
        }

        //https://stackoverflow.com/questions/10438258/using-microsoft-build-evaluation-to-publish-a-database-project-sqlproj
        //https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.dac.dacdeployoptions(v=sql.120).aspx

        //    //https://brettwgreen.com/2016/09/16/build-a-testing-database-with-ssdt-and-nuget/
        //    [SetUpFixture]
        //    public class TestDatabaseSetup
        //    {
        //        private string DatabaseConnectionString = @&quot;Data Source = (LocalDB)mssqllocaldb; Initial Catalog = master; Integrated Security = True & quot;;
        //private string DatabaseTargetName = &quot; AcmeDatabase&quot;;
        //private const string VersionTag = &quot; DacVersion&quot;;

        //[SetUp]
        //        public void SetupLocalDb()
        //        {
        //            var rebuildDatabase = true;

        //            if (!rebuildDatabase)
        //            {
        //                return;
        //            }

        //            var upgradeExisting = false;
        //            using (var connection = new SqlConnection(DatabaseConnectionString))
        //            {
        //                connection.Open();

        //                var sql = string.Format(&quot; select name from sys.databases where name = '{0}' & quot;, DatabaseTargetName);
        //                var cmd = connection.CreateCommand();
        //                cmd.CommandText = sql;
        //                var result = cmd.ExecuteScalar();
        //                upgradeExisting = result != null;
        //                cmd.Dispose();
        //            }

        //            var instance = new DacServices(DatabaseConnectionString);
        //            var path = Path.GetFullPath(@&quot; SSDT.Poseidon.dacpac & quot;);
        //            var versionPresent = false;

        //            using (var dacpac = DacPackage.Load(path))
        //            {
        //                var dacVersion = new Version(dacpac.Version.Major, dacpac.Version.Minor, dacpac.Version.Build);

        //                if (upgradeExisting)
        //                {
        //                    var dbVersion = new Version(0, 0, 0);
        //                    using (var connection = new SqlConnection(DatabaseConnectionString))
        //                    {
        //                        connection.Open();

        //                        var sql = string.Format(&quot; select value from { 0}.sys.extended_properties where name = '{1}' & quot;, DatabaseTargetName, VersionTag);
        //                        var cmd = connection.CreateCommand();
        //                        cmd.CommandText = sql;
        //                        var result = cmd.ExecuteScalar();
        //                        if (result != null)
        //                        {
        //                            dbVersion = new Version(result.ToString());
        //                            versionPresent = true;
        //                        }
        //                        cmd.Dispose();
        //                        if (dacVersion & lt;= dbVersion)
        //                {
        //                            Console.WriteLine(&quot; Database { 0}, Db Version { 1}, Dac Version { 2}... declining to apply dacpac&quot;, DatabaseTargetName, dbVersion, dacVersion);
        //                            return;
        //                        }

        //                    }
        //                }

        //                var options = new DacDeployOptions();
        //                options.DropExtendedPropertiesNotInSource = false;
        //                try
        //                {
        //                    instance.Deploy(dacpac, DatabaseTargetName, upgradeExisting, options);
        //                    var procName = (upgradeExisting & amp; &amp; versionPresent) ? &quot; sys.sp_updateextendedproperty & quot; : &quot; sys.sp_addextendedproperty & quot; ;

        //                    using (var connection = new SqlConnection(DatabaseConnectionString))
        //                    {
        //                        connection.Open();
        //                        var cmd = connection.CreateCommand();
        //                        cmd.CommandText = string.Format(&quot; EXEC { 0}.{ 1}
        //                        @name = '{2}', @value = '{3}' & quot;, DatabaseTargetName, procName, VersionTag, dacVersion.ToString());
        //                        cmd.ExecuteNonQuery();
        //                        cmd.Dispose();
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine(ex);
        //                }
        //            }

        //        }
        //    }

            //looks like this is the new way to do it..
            // https://www.nuget.org/packages/Microsoft.Data.Tools.Msbuild
            // https://blogs.msdn.microsoft.com/ssdt/2016/08/22/releasing-ssdt-with-visual-studio-15-preview-4-and-introducing-ssdt-msbuild-nuget-package/
            // https://brettwgreen.com/2016/09/16/build-a-testing-database-with-ssdt-and-nuget/

            // can try this..
            // https://social.msdn.microsoft.com/Forums/sqlserver/en-US/4cd87c1d-a4e1-493d-80dd-10add488439b/using-build-framework-to-build-and-publish-ssdt-project-in-c?forum=ssdt

            //doesn't work as is...
            //https://developercommunity.visualstudio.com/content/problem/43569/ssdt-sql-project-with-visual-studio-build-tools-20.html
            //https://developercommunity.visualstudio.com/content/problem/166536/ssdt-not-present-in-visual-studio-build-tools-2017.html

            //https://stackoverflow.com/questions/10438258/using-microsoft-build-evaluation-to-publish-a-database-project-sqlproj
            // publish profile has been set to always recreate database (supply as prop instead?)
            //https://stackoverflow.com/questions/43495509/how-to-use-buildmanager-to-build-net-core-project-or-solution-on-visual-studio

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
