using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.ProviderCommitments.Data;
using SFA.DAS.UnitOfWork;
using StructureMap;

namespace SFA.DAS.ProviderCommitments.DependencyResolution
{
    public class DatabaseRegistry : Registry
    {
        public DatabaseRegistry()
        {
            For<SqlConnection>().Use(c =>
                new SqlConnection(c.GetInstance<ProviderCommitmentsDatabaseConfiguration>().ConnectionString));
            For<ProviderDbContext>().Use(c => GetDbContext(c));
        }

        private ProviderDbContext GetDbContext(IContext context)
        {
            var dbConnection = context.GetInstance<SqlConnection>();
            return new ProviderDbContext(dbConnection);
        }
    }
}
