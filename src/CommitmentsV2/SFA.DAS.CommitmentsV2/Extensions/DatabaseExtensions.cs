using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using Azure.Core;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class DatabaseExtensions
{
    private const string AzureResource = "https://database.windows.net/";

    public static DbConnection GetSqlConnection(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        bool useManagedIdentity = !connectionStringBuilder.IntegratedSecurity && string.IsNullOrEmpty(connectionStringBuilder.UserID);

        if (!useManagedIdentity)
        {
            return new SqlConnection(connectionString);
        }

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

        return sqlConn;
    }
}