using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using Azure.Core;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class DatabaseExtensions
{
    private const string AzureResource = "https://database.windows.net/";

    public static DbConnection GetSqlConnection(string connectionString, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("SQLConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            logger.LogInformation("SQL Connection is MISSING");
            throw new ArgumentNullException(nameof(connectionString));
        }

        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        bool useManagedIdentity = !connectionStringBuilder.IntegratedSecurity && string.IsNullOrEmpty(connectionStringBuilder.UserID);

        if (!useManagedIdentity)
        {
            logger.LogInformation("SQL Connection is NOT using Managed Identity");
            return new SqlConnection(connectionString);
        }

        logger.LogInformation("SQL Connection IS using Managed Identity");
        var azureServiceTokenProvider = new ChainedTokenCredential(
            new ManagedIdentityCredential(),
            new AzureCliCredential(),
            new VisualStudioCodeCredential(),
            new VisualStudioCredential());

        var sqlConn = new SqlConnection
        {
            ConnectionString = connectionString,
            AccessToken = azureServiceTokenProvider.GetToken(new TokenRequestContext(scopes: new string[] { AzureResource })).Token
        };

        logger.LogInformation("SQL Connection string IS {0}", connectionString);
        logger.LogInformation("SQL AccessToken IS {0}", sqlConn.AccessToken);

        return sqlConn;
    }
}