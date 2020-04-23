﻿using SFA.DAS.CommitmentsV2.Data;
using System.Data.Common;
using System.Data.SqlClient;
using StructureMap;
using SFA.DAS.CommitmentsV2.Configuration;
using StructureMap.Pipeline;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class DataRegistry : Registry
    {
        public DataRegistry()
        {
            For<DbConnection>().Use(c => new SqlConnection(c.GetInstance<CommitmentsV2Configuration>().DatabaseConnectionString));
            For<ProviderCommitmentsDbContext>().Use(c => c.GetInstance<IDbContextFactory>().CreateDbContext());
            For<IProviderCommitmentsDbContext>().Use(c => c.GetInstance<IDbContextFactory>().CreateDbContext());
        }
    }
}