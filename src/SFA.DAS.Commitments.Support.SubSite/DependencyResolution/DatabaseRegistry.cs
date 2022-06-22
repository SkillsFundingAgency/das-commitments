using System;
using SFA.DAS.CommitmentsV2.Data;
using System.Data.Common;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using StructureMap;
using SFA.DAS.Configuration;
using SFA.DAS.Commitments.Support.SubSite.Configuration;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class DatabaseRegistry : Registry
    {
        private const string AzureResource = "https://database.windows.net/";

        public DatabaseRegistry()
        {
            var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName);

            For<IDbContextFactory>().Use<DbContextFactory>();

            For<DbConnection>().Use($"Build DbConnection", c =>
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                return !string.IsNullOrWhiteSpace(environmentName) && environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase)
                    ? new SqlConnection(GetConnectionString(c))
                    : new SqlConnection
                    {
                        ConnectionString = GetConnectionString(c),
                        AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result
                    };
            });

            For<ProviderCommitmentsDbContext>().Use(c => c.GetInstance<IDbContextFactory>().CreateDbContext());
        }

        private string GetConnectionString(IContext context)
        {
            return context.GetInstance<CommitmentSupportSiteConfiguartion>().DatabaseConnectionString;
        }
    }
}