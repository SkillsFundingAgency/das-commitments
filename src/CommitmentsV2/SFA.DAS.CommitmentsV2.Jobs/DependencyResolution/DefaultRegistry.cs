using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Jobs.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<ImportProvidersJobs>();
            For<IDbContextFactory>().Use<DbContextFactory>();
        }
    }
}