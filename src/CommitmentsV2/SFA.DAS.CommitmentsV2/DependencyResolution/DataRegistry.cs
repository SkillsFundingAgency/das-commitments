using System;
using SFA.DAS.CommitmentsV2.Data;
using System.Data.Common;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using StructureMap;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class DataRegistry : Registry
    {
        private const string AzureResource = "https://database.windows.net/";

        public DataRegistry()
        {
            var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName);

            For<DbConnection>().Use($"Build DbConnection", c => {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                return environmentName.Equals("L", StringComparison.CurrentCultureIgnoreCase)
                    ? new SqlConnection(GetConnectionString(c))
                    : new SqlConnection
                    {
                        ConnectionString = GetConnectionString(c),
                        AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result
                    };
            });

            For<ProviderCommitmentsDbContext>().Use(c => c.GetInstance<IDbContextFactory>().CreateDbContext());
            For<IProviderCommitmentsDbContext>().Use(c => c.GetInstance<IDbContextFactory>().CreateDbContext());
        }

        private string GetConnectionString(IContext context)
        {
            return context.GetInstance<CommitmentsV2Configuration>().DatabaseConnectionString;
        }
    }
}