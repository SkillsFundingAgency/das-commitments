using SFA.DAS.CommitmentsV2.DependencyResolution;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Jobs.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<ApprenticeshipInfoServiceRegistry>();
            registry.IncludeRegistry<ConfigurationRegistry>();
            registry.IncludeRegistry<DataRegistry>();
            registry.IncludeRegistry<DefaultRegistry>();
        }
    }
}
