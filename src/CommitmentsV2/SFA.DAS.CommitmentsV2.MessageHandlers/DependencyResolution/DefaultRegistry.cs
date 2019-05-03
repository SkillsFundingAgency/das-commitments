using SFA.DAS.CommitmentsV2.Data;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IDbContextFactory>().Use<SynchronizedDbContextFactory>();
        }
    }
}