using SFA.DAS.CommitmentsV2.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using StructureMap;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class DataRegistry : Registry
    {
        public DataRegistry()
        {
            For<IDbContextFactory>().Use<DbContextFactory>();
            For<DbConnection>().Use(c => new SqlConnection(c.GetInstance<IOptions<CommitmentsV2Configuration>>().Value.DatabaseConnectionString));
            For<ProviderCommitmentsDbContext>().Use(c => c.GetInstance<IDbContextFactory>().CreateAccountsDbContext());
        }
    }
}
