using SFA.DAS.ProviderCommitments.Data;
using SFA.DAS.ProviderCommitments.DependencyResolution;
using SFA.DAS.UnitOfWork.EntityFrameworkCore;
using SFA.DAS.UnitOfWork.SqlServer;
using StructureMap;

namespace SFA.DAS.ProviderCommitments.Web.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                c.AddRegistry<ConfigurationRegistry>();
                c.AddRegistry<DatabaseRegistry>();
                c.AddRegistry<DefaultRegistry>();
                c.AddRegistry<LoggerRegistry>();
                c.AddRegistry<EntityFrameworkCoreUnitOfWorkRegistry<ProviderDbContext>>();
                c.AddRegistry<SqlServerUnitOfWorkRegistry>();
            });
        }
    }
}