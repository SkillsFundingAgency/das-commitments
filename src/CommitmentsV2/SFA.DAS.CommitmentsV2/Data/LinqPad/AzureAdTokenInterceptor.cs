using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace SFA.DAS.CommitmentsV2.Data.LinqPad;

public class AzureAdTokenInterceptor : DbConnectionInterceptor
{
    private readonly TokenCredential _credential = new DefaultAzureCredential();
    private readonly TokenRequestContext _ctx = new(["https://database.windows.net/.default"]);

    public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
    {
        if (connection is SqlConnection sql)
        {
            var csb = new SqlConnectionStringBuilder(sql.ConnectionString);

            bool isIntegrated = csb.IntegratedSecurity;
            bool hasSqlAuth = !string.IsNullOrWhiteSpace(csb.UserID) || !string.IsNullOrWhiteSpace(csb.Password);
            bool hasAuthKeyword = csb.TryGetValue("Authentication", out var authObj) && authObj is string auth && !string.IsNullOrWhiteSpace(auth);

            if (!isIntegrated && !hasSqlAuth && !hasAuthKeyword)
            {
                // get a fresh token right before opening (handles expiry/refresh)
                var token = _credential.GetToken(_ctx, default);
                sql.AccessToken = token.Token;
            }
        }

        return base.ConnectionOpening(connection, eventData, result);
    }
}